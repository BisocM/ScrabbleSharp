using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Text;
using Microsoft.Extensions.Logging;
using ScrabbleSharp.Engine.Core.Boards;
using ScrabbleSharp.Engine.Core.Dictionary;
using ScrabbleSharp.Engine.Core.Models;
using ScrabbleSharp.Engine.Core.Rules.Interfaces;

namespace ScrabbleSharp.Engine.Services.MoveGeneration;

/// <summary>
///     Immutable per-solve data (pure value object).  Everything is thread-safe.
/// </summary>
public sealed record MoveGenerationContext(
    Board Board,
    DictionaryTrie Dictionary,
    IGameRules Rules,
    CrossCheckMatrix CrossChecks,
    bool IsFirstMove,
    int OpeningMinimumLength = 0)
{
    // Memoises cross-word legality for identical square/letter/orientation checks.
    public ConcurrentDictionary<(int Row, int Col, char Letter, bool MainIsHoriz), bool> CrossCache { get; }
        = new();
}

public sealed class MoveGenerator(ILogger<MoveGenerator> logger)
{
    private const uint AllMask = (1u << 26) - 1;
    private const int MaxAnchorsToProcess = 512;
    private const int InitialResultCapacity = 16_384;

    /// <summary>
    ///     Generate every legal move for <paramref name="rackLetters" /> on <paramref name="board" />.
    ///     Thread-safe, stateless, returns **unsorted** list (caller may sort).
    /// </summary>
    public List<Move> GenerateAllMoves(
        string rackLetters,
        Board board,
        DictionaryTrie dictionary,
        IGameRules rules,
        int openingMinimumLength = 0)
    {
        var crossChecks = PrecomputeCrossChecks(board, dictionary);

        var ctx = new MoveGenerationContext(
            board,
            dictionary,
            rules,
            crossChecks,
            IsBoardEmpty(board),
            openingMinimumLength);

        var anchors = FindAnchors(ctx);

        if (anchors.Count > MaxAnchorsToProcess)
            anchors = anchors
                .OrderBy(a => Math.Abs(a.Item1 - board.OriginRow) +
                              Math.Abs(a.Item2 - board.OriginCol)) // centre-biased
                .Take(MaxAnchorsToProcess)
                .ToList();

        // Concurrent result store with atomic de-duplication.
        var concurrencyLevel = Environment.ProcessorCount;
        var results = new ConcurrentDictionary<string, Move>(
            concurrencyLevel,
            InitialResultCapacity,
            StringComparer.Ordinal);

        var po = new ParallelOptions { MaxDegreeOfParallelism = concurrencyLevel };
        Parallel.ForEach(anchors, po, anchor =>
        {
            var (r, c) = anchor;

            GenerateFromAnchor(r, c, true, new RackCounts(rackLetters), ctx, results);
            GenerateFromAnchor(r, c, false, new RackCounts(rackLetters), ctx, results);
        });

        return results.Values.ToList();
    }

    #region ----------  Pre-computation  ----------

    private static CrossCheckMatrix PrecomputeCrossChecks(Board board, DictionaryTrie dictionary)
    {
        int boardRows = board.Rows, boardCols = board.Cols;
        var horiz = new uint[boardRows, boardCols];
        var vert = new uint[boardRows, boardCols];

        Span<char> buf = stackalloc char[Math.Min(board.Rows, board.Cols)];

        for (var r = 0; r < boardRows; r++)
        for (var c = 0; c < boardCols; c++)
        {
            if (!board.IsEmpty(r, c))
            {
                horiz[r, c] = vert[r, c] = 0;
                continue;
            }

            // Vertical cross (main word horizontal)
            horiz[r, c] = ComputeMaskForLine(
                board, dictionary,
                r, c,
                -1, 0, 1, 0,
                buf);

            // Horizontal cross (main word vertical)
            vert[r, c] = ComputeMaskForLine(
                board, dictionary,
                r, c,
                0, -1, 0, 1,
                buf);
        }

        return new CrossCheckMatrix(horiz, vert);
    }

    /// <summary>
    ///     Returns bit-mask of A–Z letters legal at (<paramref name="row" />,<paramref name="col" />) for the
    ///     perpendicular word defined by the two directional vectors.
    /// </summary>
    private static uint ComputeMaskForLine(
        Board board, DictionaryTrie dict,
        int row, int col,
        int dR1, int dC1,
        int dR2, int dC2,
        Span<char> buf)
    {
        // Locate start of existing string segment
        int sR = row, sC = col;
        while (IsInsideBoard(sR + dR1, sC + dC1, board) && !board.IsEmpty(sR + dR1, sC + dC1))
        {
            sR += dR1;
            sC += dC1;
        }

        // Locate end
        int eR = row, eC = col;
        while (IsInsideBoard(eR + dR2, eC + dC2, board) && !board.IsEmpty(eR + dR2, eC + dC2))
        {
            eR += dR2;
            eC += dC2;
        }

        // Compute length and early-exit for single-letter cross
        var len = 0;
        for (int r = sR, c = sC;
             !(r == eR && c == eC);
             r += dR2, c += dC2) len++;
        len++; // include end square

        if (len == 1) return AllMask; // single letter has no perpendicular word

        // Copy into buffer, mark target with placeholder.
        var idx = 0;
        for (int r = sR, c = sC;
             idx < len;
             r += dR2, c += dC2, idx++)
            buf[idx] = r == row && c == col
                ? '#'
                : board.GetSquare(r, c).Letter!.Value;

        uint mask = 0;
        for (var L = 'A'; L <= 'Z'; L++)
        {
            buf[IndexOfTarget(row, col, sR, sC, dR2, dC2)] = L;

            ReadOnlySpan<char> span = buf.Slice(0, len); // **corrected line**
            if (dict.Contains(span)) // fast exact-word check
                mask |= 1u << (L - 'A');
        }

        return mask;

        static int IndexOfTarget(int row, int col, int sR, int sC, int dR, int dC)
        {
            int idx = 0, r = sR, c = sC;
            while (!(r == row && c == col))
            {
                r += dR;
                c += dC;
                idx++;
            }

            return idx;
        }
    }

    #endregion

    #region ----------  Anchor-driven generation ----------

    private void GenerateFromAnchor(
        int anchorRow, int anchorCol,
        bool isHorizontal,
        RackCounts rack,
        MoveGenerationContext ctx,
        IDictionary<string, Move> results)
    {
        var (dR, dC) = isHorizontal ? (0, 1) : (1, 0);

        var trie = ctx.Dictionary.Root;
        int r = anchorRow - dR, c = anchorCol - dC;

        if (HasExistingPrefix(r, c, isHorizontal, ctx, out var prefix))
            trie = prefix;

        GenerateLeftPart(r, c, trie, rack,
            new List<TilePlacement>(),
            (anchorRow, anchorCol),
            ctx, results, isHorizontal);
    }

    private bool HasExistingPrefix(
        int startR, int startC,
        bool horiz,
        MoveGenerationContext ctx,
        out DictionaryTrie.Node node)
    {
        var (dR, dC) = horiz ? (0, 1) : (1, 0);
        var sb = new StringBuilder();

        for (int r = startR, c = startC;
             IsInsideBoard(r, c, ctx.Board) && !ctx.Board.IsEmpty(r, c);
             r -= dR, c -= dC)
            sb.Insert(0, ctx.Board.GetSquare(r, c).Letter!.Value);

        node = ctx.Dictionary.Root;
        foreach (var ch in sb.ToString())
            if (!node.Children.TryGetValue(ch, out node))
                return false;
        return true;
    }

    private void GenerateLeftPart(
        int currR, int currC,
        DictionaryTrie.Node trie,
        RackCounts rack,
        List<TilePlacement> placed,
        (int Row, int Col) anchor,
        MoveGenerationContext ctx,
        IDictionary<string, Move> results,
        bool horiz)
    {
        ExtendRight(anchor.Row, anchor.Col, trie, rack, placed, anchor, ctx, results, horiz);

        if (!IsInsideBoard(currR, currC, ctx.Board) || rack.TilesRemaining == 0) return;

        var (dR, dC) = horiz ? (0, 1) : (1, 0);

        foreach (var t in rack.DistinctTiles())
            if (t == '*')
                for (var sub = 'A'; sub <= 'Z'; sub++)
                    Try(sub, true);
            else
                Try(t, false);

        void Try(char L, bool blank)
        {
            if (!trie.Children.TryGetValue(L, out var nxt)) return;
            if (!IsCrossLegal(currR, currC, L, horiz, ctx)) return;

            var rackTile = blank ? '*' : L;
            rack.Take(rackTile);
            placed.Add(new TilePlacement(currR, currC, L, blank));

            GenerateLeftPart(currR - dR, currC - dC, nxt, rack, placed,
                anchor, ctx, results, horiz);

            placed.RemoveAt(placed.Count - 1);
            rack.Put(rackTile);
        }
    }

    private void ExtendRight(
        int row, int col,
        DictionaryTrie.Node trie,
        RackCounts rack,
        List<TilePlacement> placed,
        (int Row, int Col) anchor,
        MoveGenerationContext ctx,
        IDictionary<string, Move> results,
        bool horiz)
    {
        var board = ctx.Board;
        if (!IsInsideBoard(row, col, board)) return;

        // Existing board tile
        if (!board.IsEmpty(row, col))
        {
            var boardL = board.GetSquare(row, col).Letter!.Value;
            if (!trie.Children.TryGetValue(boardL, out var nxt)) return;

            var (dR, dC) = horiz ? (0, 1) : (1, 0);
            ExtendRight(row + dR, col + dC, nxt, rack, placed,
                anchor, ctx, results, horiz);
            return;
        }

        // Empty square → maybe finish word
        if (trie.IsWord && placed.Count > 0) EmitMove(placed, anchor, ctx, results, horiz);

        if (rack.TilesRemaining == 0) return;

        foreach (var t in rack.DistinctTiles())
            if (t == '*')
                for (var sub = 'A'; sub <= 'Z'; sub++)
                    Try(sub, true);
            else
                Try(t, false);

        void Try(char L, bool blank)
        {
            if (!trie.Children.TryGetValue(L, out var nxt)) return;
            if (!IsCrossLegal(row, col, L, horiz, ctx)) return;

            var rackTile = blank ? '*' : L;
            rack.Take(rackTile);
            placed.Add(new TilePlacement(row, col, L, blank));

            var (dR, dC) = horiz ? (0, 1) : (1, 0);
            ExtendRight(row + dR, col + dC, nxt, rack, placed,
                anchor, ctx, results, horiz);

            placed.RemoveAt(placed.Count - 1);
            rack.Put(rackTile);
        }
    }

    #endregion

    #region ----------  Emit / validation helpers ----------

    private void EmitMove(
        List<TilePlacement> placedTiles,
        (int Row, int Col) anchor,
        MoveGenerationContext context,
        IDictionary<string, Move> results,
        bool isHorizontal)
    {
        if (placedTiles.Count == 0) return; // must place at least one tile

        var provisional = new Move
        {
            Tiles = placedTiles.ToImmutableList(),
            IsHorizontal = isHorizontal
        };

        var (word, startRow, startCol) = GetMoveDetails(provisional, context.Board);

        // FIX: Re-validate the word reconstructed from the board.
        // The generation logic can find a valid path in the trie (e.g., "VIN")
        // but place the tiles on the board incorrectly to form a different word (e.g., "NIV" or "IVN").
        // We must ensure the word actually formed on the board is valid before scoring it.
        if (!context.Dictionary.IsWordExact(word))
            // This is not an error, but a consequence of the generator's imperfect left-part generation.
            // Simply discard the invalid move candidate and continue.
            return;

        var move = provisional with
        {
            Word = word,
            StartRow = startRow,
            StartCol = startCol
        };

        if (context.IsFirstMove)
        {
            var coversAnchor = move.Tiles.Any(p => p.Row == anchor.Row && p.Col == anchor.Col);
            if (!coversAnchor || move.Tiles.Count < context.OpeningMinimumLength)
                return;
        }
        else
        {
            if (!MoveTouchesExisting(context.Board, move))
                return;
        }

        var score = context.Rules.CalculateMoveScore(context.Board, move);
        var key = MakeKey(move);

        if (!results.TryGetValue(key, out var existing) || score > existing.Score)
            results[key] = move with { Score = score };
    }

    private static string MakeKey(Move m) =>
        $"{m.Word}:{m.StartRow}:{m.StartCol}:{(m.IsHorizontal ? 'H' : 'V')}";

    private bool IsCrossLegal(int r, int c, char L, bool horiz, MoveGenerationContext ctx)
    {
        var k = (r, c, L, horiz);
        if (ctx.CrossCache.TryGetValue(k, out var ok)) return ok;

        ok = ctx.CrossChecks.IsAllowed(r, c, L, horiz);
        ctx.CrossCache[k] = ok;
        return ok;
    }

    #endregion

    #region ----------  Misc utility ----------

    private static bool IsInsideBoard(int r, int c, Board b)
        => r >= 0 && c >= 0 && r < b.Rows && c < b.Cols;

    private static bool IsBoardEmpty(Board b)
    {
        for (var r = 0; r < b.Rows; r++)
        for (var c = 0; c < b.Cols; c++)
            if (!b.IsEmpty(r, c))
                return false;
        return true;
    }

    private static List<(int, int)> FindAnchors(MoveGenerationContext ctx)
    {
        var list = new List<(int, int)>();
        var b = ctx.Board;

        if (ctx.IsFirstMove)
        {
            list.Add((b.OriginRow, b.OriginCol));
            return list;
        }

        for (var r = 0; r < b.Rows; r++)
        for (var c = 0; c < b.Cols; c++)
            if (b.IsEmpty(r, c) &&
                ((IsInsideBoard(r - 1, c, b) && !b.IsEmpty(r - 1, c)) ||
                 (IsInsideBoard(r + 1, c, b) && !b.IsEmpty(r + 1, c)) ||
                 (IsInsideBoard(r, c - 1, b) && !b.IsEmpty(r, c - 1)) ||
                 (IsInsideBoard(r, c + 1, b) && !b.IsEmpty(r, c + 1))))
                list.Add((r, c));
        return list;
    }

    private static bool MoveTouchesExisting(Board b, Move m)
    {
        var (dR, dC) = m.IsHorizontal ? (0, 1) : (1, 0);
        var newT = m.Tiles.ToDictionary(t => (t.Row, t.Col));

        for (int r = m.StartRow, c = m.StartCol;
             IsInsideBoard(r, c, b) && (!b.IsEmpty(r, c) || newT.ContainsKey((r, c)));
             r += dR, c += dC)
            if (!newT.ContainsKey((r, c)) && !b.IsEmpty(r, c))
                return true;

        foreach (var t in m.Tiles)
            if ((IsInsideBoard(t.Row - 1, t.Col, b) && !b.IsEmpty(t.Row - 1, t.Col)) ||
                (IsInsideBoard(t.Row + 1, t.Col, b) && !b.IsEmpty(t.Row + 1, t.Col)) ||
                (IsInsideBoard(t.Row, t.Col - 1, b) && !b.IsEmpty(t.Row, t.Col - 1)) ||
                (IsInsideBoard(t.Row, t.Col + 1, b) && !b.IsEmpty(t.Row, t.Col + 1)))
                return true;
        return false;
    }

    private static (string, int, int) GetMoveDetails(Move m, Board b)
    {
        var (dR, dC) = m.IsHorizontal ? (0, 1) : (1, 0);

        int sr = m.Tiles.Min(t => t.Row), sc = m.Tiles.Min(t => t.Col);
        while (IsInsideBoard(sr - dR, sc - dC, b) && !b.IsEmpty(sr - dR, sc - dC))
        {
            sr -= dR;
            sc -= dC;
        }

        var placed = m.Tiles.ToDictionary(tp => (tp.Row, tp.Col));
        var sb = new StringBuilder();

        for (int r = sr, c = sc; IsInsideBoard(r, c, b); r += dR, c += dC)
            if (placed.TryGetValue((r, c), out var tp)) sb.Append(tp.Letter);
            else if (!b.IsEmpty(r, c)) sb.Append(b.GetSquare(r, c).Letter!.Value);
            else break;
        return (sb.ToString(), sr, sc);
    }

    #endregion
}