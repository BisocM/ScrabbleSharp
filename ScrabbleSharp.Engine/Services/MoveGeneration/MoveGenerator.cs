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
///     Encapsulates the context for a single move generation process.
/// </summary>
public sealed record MoveGenerationContext(
    Board Board,
    DictionaryTrie Dictionary,
    IGameRules Rules,
    CrossCheckMatrix CrossChecks,
    bool IsFirstMove,
    int OpeningMinimumLength = 0)
{
    /// <summary>
    ///     Caches the results of cross-check lookups to avoid redundant computations within a single generation pass.
    /// </summary>
    public ConcurrentDictionary<(int Row, int Col, char Letter, bool MainIsHoriz), bool> CrossCache { get; }
        = new();
}

/// <summary>
///     The primary engine for generating all possible legal moves from a given board state and rack.
/// </summary>
/// <remarks>
///     This implementation uses a classic algorithm based on "anchors". An anchor is an empty square
///     adjacent to an existing tile. The generator iterates through all anchors and, for each,
///     generates all possible words that can be played through that anchor.
/// </remarks>
public sealed class MoveGenerator(ILogger<MoveGenerator> logger)
{
    private const uint AllMask = (1u << 26) - 1;
    private const int MaxAnchorsToProcess = 512;
    private const int InitialResultCapacity = 16_384;

    /// <summary>
    ///     Generates all possible legal moves for a given rack and board state.
    /// </summary>
    /// <param name="rackLetters">A string representing the letters on the player's rack (e.g., "HELLO**").</param>
    /// <param name="board">The current board state.</param>
    /// <param name="dictionary">The dictionary trie to validate words against.</param>
    /// <param name="rules">The game rules for scoring.</param>
    /// <param name="openingMinimumLength">The minimum length for the first move of the game.</param>
    /// <returns>A list of all valid <see cref="Move" /> objects.</returns>
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

        // To prevent performance issues on very open boards, limit the number of anchors processed.
        // A center-bias sort helps prioritize more promising anchors.
        if (anchors.Count > MaxAnchorsToProcess)
            anchors = anchors
                .OrderBy(a => Math.Abs(a.Item1 - board.OriginRow) +
                              Math.Abs(a.Item2 - board.OriginCol)) // centre-biased
                .Take(MaxAnchorsToProcess)
                .ToList();

        var concurrencyLevel = Environment.ProcessorCount;
        var resultsBag = new ConcurrentBag<List<Move>>();
        var po = new ParallelOptions { MaxDegreeOfParallelism = concurrencyLevel };

        // Use a parallel foreach with thread-local storage to minimize lock contention.
        Parallel.ForEach(anchors, po,
            () => new List<Move>(InitialResultCapacity / concurrencyLevel), // Init thread-local list
            (anchor, _, localList) =>
            {
                // Each thread gets its own dictionary to avoid concurrent writes.
                var threadResults = new Dictionary<string, Move>();
                var (r, c) = anchor;

                // Generate both horizontal and vertical moves from each anchor.
                GenerateFromAnchor(r, c, true, new RackCounts(rackLetters), ctx, threadResults);
                GenerateFromAnchor(r, c, false, new RackCounts(rackLetters), ctx, threadResults);

                localList.AddRange(threadResults.Values);
                return localList;
            },
            resultsBag.Add // Add the completed thread-local list to the bag
        );

        // Merge results from all threads into a final dictionary to handle duplicates across threads.
        var finalResults = new Dictionary<string, Move>(InitialResultCapacity);
        foreach (var list in resultsBag)
        foreach (var move in list)
        {
            var key = MakeKey(move);
            if (!finalResults.TryGetValue(key, out var existing) || move.Score > existing.Score)
                finalResults[key] = move;
        }

        return finalResults.Values.ToList();
    }

    /// <summary>
    ///     Pre-computes the <see cref="CrossCheckMatrix" /> for the entire board.
    /// </summary>
    private static CrossCheckMatrix PrecomputeCrossChecks(Board board, DictionaryTrie dictionary)
    {
        int boardRows = board.Rows, boardCols = board.Cols;
        var horiz = new uint[boardRows, boardCols];
        var vert = new uint[boardRows, boardCols];

        Span<char> buf = stackalloc char[Math.Max(board.Rows, board.Cols)];

        for (var r = 0; r < boardRows; r++)
        for (var c = 0; c < boardCols; c++)
        {
            if (!board.IsEmpty(r, c))
            {
                horiz[r, c] = vert[r, c] = 0; // No placement possible
                continue;
            }

            // Mask for vertical cross-words (when main move is horizontal)
            horiz[r, c] = ComputeMaskForLine(
                board, dictionary,
                r, c,
                -1, 0, 1, 0, // Above and below
                buf);

            // Mask for horizontal cross-words (when main move is vertical)
            vert[r, c] = ComputeMaskForLine(
                board, dictionary,
                r, c,
                0, -1, 0, 1, // Left and right
                buf);
        }

        return new CrossCheckMatrix(horiz, vert);
    }

    /// <summary>
    ///     Computes the cross-check mask for a single empty square by intelligently traversing the dictionary trie.
    /// </summary>
    private static uint ComputeMaskForLine(
        Board board, DictionaryTrie dict,
        int row, int col,
        int dR1, int dC1, // Direction for prefix
        int dR2, int dC2, // Direction for suffix
        Span<char> buf)
    {
        // Find the start and end of the potential cross-word.
        int sR = row, sC = col;
        while (IsInsideBoard(sR + dR1, sC + dC1, board) && !board.IsEmpty(sR + dR1, sC + dC1))
        {
            sR += dR1;
            sC += dC1;
        }

        int eR = row, eC = col;
        while (IsInsideBoard(eR + dR2, eC + dC2, board) && !board.IsEmpty(eR + dR2, eC + dC2))
        {
            eR += dR2;
            eC += dC2;
        }

        // If there's no existing word part, any letter can be placed.
        if (sR == row && sC == col && eR == row && eC == col) return AllMask;

        // Extract the prefix (letters before the empty square).
        var prefixLen = 0;
        for (int r = sR, c = sC; r != row || c != col; r += dR2, c += dC2)
            buf[prefixLen++] = board.GetSquare(r, c).Letter!.Value;
        var prefixSpan = buf.Slice(0, prefixLen);

        // Extract the suffix (letters after the empty square).
        var suffixLen = 0;
        for (int r = row + dR2, c = col + dC2; r != eR + dR2 || c != eC + dC2; r += dR2, c += dC2)
            buf[suffixLen++] = board.GetSquare(r, c).Letter!.Value;
        var suffixSpan = buf.Slice(0, suffixLen);

        // 1. Traverse to the end of the prefix.
        var prefixNode = FindNode(dict.Root, prefixSpan);
        if (prefixNode is null) return 0; // Prefix doesn't exist.

        uint mask = 0;
        for (var L = 'A'; L <= 'Z'; L++)
            // 2. Check if the current letter is a valid child.
            if (prefixNode.Children.TryGetValue(L, out var nextNode))
                // 3. From the new node, check if the suffix completes a word.
                if (SuffixFormsWord(nextNode, suffixSpan))
                    mask |= 1u << (L - 'A');

        return mask;
    }

    /// <summary>
    ///     Traverses a trie from a start node to find the node corresponding to a prefix.
    /// </summary>
    private static DictionaryTrie.Node? FindNode(DictionaryTrie.Node startNode, ReadOnlySpan<char> prefix)
    {
        var currentNode = startNode;
        foreach (var character in prefix)
            if (!currentNode.Children.TryGetValue(character, out currentNode))
                return null;

        return currentNode;
    }

    /// <summary>
    ///     Checks if a suffix exists from a start node and forms a valid word.
    /// </summary>
    private static bool SuffixFormsWord(DictionaryTrie.Node startNode, ReadOnlySpan<char> suffix)
    {
        var currentNode = startNode;
        foreach (var character in suffix)
            if (!currentNode.Children.TryGetValue(character, out currentNode))
                return false;

        return currentNode.IsWord;
    }

    /// <summary>
    ///     Kicks off the recursive generation process for a single anchor.
    /// </summary>
    private void GenerateFromAnchor(
        int anchorRow, int anchorCol,
        bool isHorizontal,
        RackCounts rack,
        MoveGenerationContext ctx,
        IDictionary<string, Move> results)
    {
        var (dR, dC) = isHorizontal ? (0, 1) : (1, 0);

        int r = anchorRow - dR, c = anchorCol - dC;

        // If there's an existing word part before the anchor, advance the trie node.
        if (HasExistingPrefix(r, c, isHorizontal, ctx, out var trie))
            GenerateLeftPart(r, c, trie, rack,
                new List<TilePlacement>(),
                (anchorRow, anchorCol),
                ctx, results, isHorizontal);
    }

    /// <summary>
    ///     Traverses the trie for an existing prefix of letters on the board without string allocations.
    /// </summary>
    /// <summary>
    ///     Traverses the trie for an existing prefix of letters on the board without string allocations.
    /// </summary>
    private bool HasExistingPrefix(
        int startR, int startC,
        bool horiz,
        MoveGenerationContext ctx,
        out DictionaryTrie.Node node)
    {
        node = ctx.Dictionary.Root;
        // If the square we start checking from is off the board or empty, there can be no prefix.
        if (!IsInsideBoard(startR, startC, ctx.Board) || ctx.Board.IsEmpty(startR, startC))
            return true;

        var (dR, dC) = horiz ? (0, 1) : (1, 0);

        // Find the actual start of the prefix segment by walking backwards.
        int prefixStartR = startR, prefixStartC = startC;
        while (IsInsideBoard(prefixStartR - dR, prefixStartC - dC, ctx.Board) &&
               !ctx.Board.IsEmpty(prefixStartR - dR, prefixStartC - dC))
        {
            prefixStartR -= dR;
            prefixStartC -= dC;
        }

        // Now walk forward from the real start of the prefix, traversing the trie.
        for (int r = prefixStartR, c = prefixStartC;; r += dR, c += dC)
        {
            var letter = ctx.Board.GetSquare(r, c).Letter!.Value;
            if (!node.Children.TryGetValue(letter, out node))
            {
                // This case should not be reachable if the board contains valid words.
                return false;
            }

            // We've processed the last character of the prefix; break successfully.
            if (r == startR && c == startC)
                break;
        }

        return true;
    }


    /// <summary>
    ///     Recursively generates the part of a word to the left of (or above) an anchor.
    /// </summary>
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

        if (!IsInsideBoard(currR, currC, ctx.Board) || rack.TilesRemaining == 0 || placed.Count > 0) return;

        // Define the direction vectors based on the move's orientation.
        var (dR, dC) = horiz ? (0, 1) : (1, 0);

        foreach (var t in rack.DistinctTiles())
            if (t == '*') // Handle blanks
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

            // The recursive call now correctly references dR and dC.
            GenerateLeftPart(currR - dR, currC - dC, nxt, rack, placed,
                anchor, ctx, results, horiz);

            placed.RemoveAt(placed.Count - 1);
            rack.Put(rackTile);
        }
    }

    /// <summary>
    ///     Recursively generates the part of a word from the anchor onwards (right or down).
    /// </summary>
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

        if (!board.IsEmpty(row, col))
        {
            // If the square is occupied, traverse the trie with the existing letter.
            var boardL = board.GetSquare(row, col).Letter!.Value;
            if (!trie.Children.TryGetValue(boardL, out var nxt)) return;

            var (dR, dC) = horiz ? (0, 1) : (1, 0);
            ExtendRight(row + dR, col + dC, nxt, rack, placed,
                anchor, ctx, results, horiz);
            return;
        }

        // If we've formed a valid word and placed at least one tile, emit it.
        if (trie.IsWord && placed.Count > 0) EmitMove(placed, anchor, ctx, results, horiz);

        if (rack.TilesRemaining == 0) return;

        // Try placing each available rack tile on the current empty square.
        foreach (var t in rack.DistinctTiles())
            if (t == '*') // Handle blanks
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

            // Backtrack
            placed.RemoveAt(placed.Count - 1);
            rack.Put(rackTile);
        }
    }

    /// <summary>
    ///     Finalizes and records a valid move.
    /// </summary>
    private void EmitMove(
        List<TilePlacement> placedTiles,
        (int Row, int Col) anchor,
        MoveGenerationContext context,
        IDictionary<string, Move> results,
        bool isHorizontal)
    {
        if (placedTiles.Count == 0) return;

        var provisional = new Move
        {
            Tiles = placedTiles.ToImmutableList(),
            IsHorizontal = isHorizontal
        };

        var (word, startRow, startCol) = GetMoveDetails(provisional, context.Board);

        var move = provisional with
        {
            Word = word,
            StartRow = startRow,
            StartCol = startCol
        };

        // Apply first-move-specific constraints
        if (context.IsFirstMove)
        {
            var coversAnchor = move.Tiles.Any(p => p.Row == anchor.Row && p.Col == anchor.Col);
            if (!coversAnchor || move.Tiles.Count < context.OpeningMinimumLength)
                return;
        }
        else // Subsequent moves must touch an existing tile
        {
            if (!MoveTouchesExisting(context.Board, move))
                return;
        }

        var score = context.Rules.CalculateMoveScore(context.Board, move);
        var key = MakeKey(move);

        // Add the move if it's new or has a higher score than an existing move for the same play.
        if (!results.TryGetValue(key, out var existing) || score > existing.Score)
            results[key] = move with { Score = score };
    }

    /// <summary>
    ///     Creates a unique key for a move to prevent duplicates in the results dictionary.
    /// </summary>
    private static string MakeKey(Move m) =>
        $"{m.Word}:{m.StartRow}:{m.StartCol}:{(m.IsHorizontal ? 'H' : 'V')}";

    /// <summary>
    ///     Checks if placing a letter is legal based on the cross-check matrix, using a cache for performance.
    /// </summary>
    private bool IsCrossLegal(int r, int c, char l, bool horiz, MoveGenerationContext ctx)
    {
        var k = (r, c, L: l, horiz);
        if (ctx.CrossCache.TryGetValue(k, out var ok)) return ok;

        ok = ctx.CrossChecks.IsAllowed(r, c, l, horiz);
        ctx.CrossCache[k] = ok;
        return ok;
    }

    private static bool IsInsideBoard(int r, int c, Board b)
        => r >= 0 && c >= 0 && r < b.Rows && c < b.Cols;

    /// <summary>
    ///     Checks if the board is completely empty.
    /// </summary>
    private static bool IsBoardEmpty(Board b)
    {
        for (var r = 0; r < b.Rows; r++)
        for (var c = 0; c < b.Cols; c++)
            if (!b.IsEmpty(r, c))
                return false;
        return true;
    }

    /// <summary>
    ///     Finds all anchor squares on the board.
    /// </summary>
    /// <remarks>
    ///     If it's the first move, the only anchor is the center square. Otherwise, an anchor
    ///     is any empty square that is adjacent (horizontally or vertically) to a tile.
    /// </remarks>
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

    /// <summary>
    ///     Verifies that a move is connected to existing tiles on the board.
    /// </summary>
    private static bool MoveTouchesExisting(Board b, Move m)
    {
        var (dR, dC) = m.IsHorizontal ? (0, 1) : (1, 0);
        var newT = m.Tiles.ToDictionary(t => (t.Row, t.Col));

        // Check if the main word incorporates any existing tiles
        for (int r = m.StartRow, c = m.StartCol;
             IsInsideBoard(r, c, b) && (!b.IsEmpty(r, c) || newT.ContainsKey((r, c)));
             r += dR, c += dC)
            if (!newT.ContainsKey((r, c)) && !b.IsEmpty(r, c))
                return true;

        // Check if any new tile is adjacent to an existing tile (forming a cross-word)
        return m.Tiles.Any(t =>
            (IsInsideBoard(t.Row - 1, t.Col, b) && !b.IsEmpty(t.Row - 1, t.Col)) || (IsInsideBoard(t.Row + 1, t.Col, b) && !b.IsEmpty(t.Row + 1, t.Col)) ||
            (IsInsideBoard(t.Row, t.Col - 1, b) && !b.IsEmpty(t.Row, t.Col - 1)) || (IsInsideBoard(t.Row, t.Col + 1, b) && !b.IsEmpty(t.Row, t.Col + 1)));
    }

    /// <summary>
    ///     Reconstructs the full details of a move (word, start position) from the list of placed tiles.
    /// </summary>
    private static (string, int, int) GetMoveDetails(Move m, Board b)
    {
        var (dR, dC) = m.IsHorizontal ? (0, 1) : (1, 0);

        // Find the true start of the word, which may be before the first placed tile.
        int sr = m.Tiles.Min(t => t.Row), sc = m.Tiles.Min(t => t.Col);
        while (IsInsideBoard(sr - dR, sc - dC, b) && !b.IsEmpty(sr - dR, sc - dC))
        {
            sr -= dR;
            sc -= dC;
        }

        var placed = m.Tiles.ToDictionary(tp => (tp.Row, tp.Col));
        var sb = new StringBuilder();

        // Build the full word string by iterating from the start position.
        for (int r = sr, c = sc; IsInsideBoard(r, c, b); r += dR, c += dC)
            if (placed.TryGetValue((r, c), out var tp)) sb.Append(tp.Letter);
            else if (!b.IsEmpty(r, c)) sb.Append(b.GetSquare(r, c).Letter!.Value);
            else break;
        return (sb.ToString(), sr, sc);
    }
}