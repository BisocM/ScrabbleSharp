using ScrabbleSharp.Engine.Core.Boards;
using ScrabbleSharp.Engine.Core.Models;
using ScrabbleSharp.Engine.Core.Rules.Interfaces;
using ScrabbleSharp.Engine.Core.Tiles;

namespace ScrabbleSharp.Engine.Core.Rules.Types;

/// <summary>
///     An abstract base class providing common implementations for game rule logic.
/// </summary>
public abstract class GameRulesBase : IGameRules
{
    /// <inheritdoc />
    public abstract int GetBaseLetterScore(char letter);

    /// <inheritdoc />
    /// <remarks>
    ///     By default, blank tiles score 0, otherwise the base letter score is returned.
    ///     This method does not account for letter multipliers on the board, as those are handled
    ///     during the main move calculation.
    /// </remarks>
    public virtual int GetLetterScore(
        Board board,
        int row,
        int col,
        char letter,
        bool isNewTile,
        bool blank)
    {
        return blank
            ? 0
            : GetBaseLetterScore(letter);
    }

    /// <inheritdoc />
    /// <remarks>
    ///     This implementation "consumes" a square's multiplier once a tile is placed on it.
    ///     The score contribution of the letter (with its multiplier) is cached in <see cref="Square.PermanentLetterScore" />,
    ///     though this property is not currently used in move calculation. The multiplier is then set to None.
    /// </remarks>
    public virtual void OnTilePlaced(
        Board board,
        int row,
        int col,
        char letter,
        bool blank)
    {
        var square = board.GetSquare(row, col);

        var effectiveScore = blank ? 0 : GetBaseLetterScore(letter);

        effectiveScore *= square.LetterMultiplier;
        square.PermanentLetterScore = effectiveScore;

        // Once a tile is placed, the multiplier is used up.
        square.SetMultiplier(MultiplierType.None);
    }

    /// <inheritdoc />
    /// <remarks>
    ///     This method calculates the score by summing the main word's score and all cross-word scores,
    ///     then applying any final bonuses (like a bingo).
    /// </remarks>
    public virtual int CalculateMoveScore(Board board, Move move)
    {
        var newTiles = move.Tiles.ToDictionary(tile => (tile.Row, tile.Col));
        var mainWordScore = 0;
        var wordMultiplier = 1;

        var (deltaRow, deltaCol) = move.IsHorizontal ? (0, 1) : (1, 0);

        // Calculate score for the main word
        for (var i = 0; i < move.Word.Length; i++)
        {
            var row = move.StartRow + i * deltaRow;
            var column = move.StartCol + i * deltaCol;
            var letter = move.Word[i];

            var isNew = newTiles.TryGetValue((row, column), out var placement);
            var isBlank = isNew && placement!.IsBlank;

            var letterScore = GetLetterScore(board, row, column, letter, isNew, isBlank);

            if (isNew)
            {
                // Apply letter and word multipliers only for newly placed tiles
                letterScore *= board.GetLetterMultiplier(row, column);
                wordMultiplier *= board.GetWordMultiplier(row, column);
            }

            mainWordScore += letterScore;
        }

        mainWordScore *= wordMultiplier;

        // Calculate scores for any cross-words formed
        var crossScore = 0;
        foreach (var tile in move.Tiles)
            crossScore += CalculateCrossWordScore(board, tile, move.IsHorizontal);

        var preBonusScore = mainWordScore + crossScore;

        return ApplyFinalBonuses(preBonusScore, move.Tiles.Count);
    }

    /// <inheritdoc />
    /// <remarks>
    ///     This default implementation awards a 50-point bonus if 7 tiles are placed (a "bingo").
    /// </remarks>
    public virtual int ApplyFinalBonuses(int preBonusScore, int tilesPlacedCount)
    {
        if (tilesPlacedCount == 7) return preBonusScore + 50;
        return preBonusScore;
    }

    /// <summary>
    ///     Calculates the score of a single cross-word formed by a newly placed tile.
    /// </summary>
    /// <param name="board">The game board.</param>
    /// <param name="placedTile">The newly placed tile that forms the cross-word.</param>
    /// <param name="mainIsHorizontal">Whether the main move is horizontal.</param>
    /// <returns>The score of the cross-word, or 0 if no cross-word was formed.</returns>
    protected virtual int CalculateCrossWordScore(Board board,
        TilePlacement placedTile,
        bool mainIsHorizontal)
    {
        // Perpendicular direction to the main move
        var (deltaRow, deltaCol) = mainIsHorizontal ? (1, 0) : (0, 1);
        int currentRow = placedTile.Row, currentCol = placedTile.Col;

        // Find the start of the cross-word
        while (currentRow - deltaRow >= 0 && currentCol - deltaCol >= 0 &&
               !board.IsEmpty(currentRow - deltaRow, currentCol - deltaCol))
        {
            currentRow -= deltaRow;
            currentCol -= deltaCol;
        }

        // Check if a valid cross-word exists (must be more than one letter long).
        if (currentRow == placedTile.Row && currentCol == placedTile.Col &&
            (currentRow + deltaRow >= board.Rows || currentCol + deltaCol >= board.Cols ||
             board.IsEmpty(currentRow + deltaRow, currentCol + deltaCol)))
            return 0; // No adjacent tiles in the cross direction, so no cross-word.

        int wordScore = 0, wordMultiplier = 1;

        // Iterate through the letters of the cross-word to calculate its score
        for (int iteratingRow = currentRow, iteratingCol = currentCol;
             iteratingRow < board.Rows && iteratingCol < board.Cols;
             iteratingRow += deltaRow, iteratingCol += deltaCol)
        {
            var isNew = iteratingRow == placedTile.Row && iteratingCol == placedTile.Col;

            char letter;
            bool blank;
            if (isNew)
            {
                letter = placedTile.Letter;
                blank = placedTile.IsBlank;
            }
            else
            {
                var square = board.GetSquare(iteratingRow, iteratingCol);
                if (square.Letter is null) break; // End of word
                letter = square.Letter.Value;
                blank = square.IsBlank;
            }

            var letterScore = GetLetterScore(board, iteratingRow, iteratingCol, letter, isNew, blank);

            if (isNew)
            {
                // Multipliers on the square of the newly placed tile apply to the cross-word as well.
                letterScore *= board.GetLetterMultiplier(iteratingRow, iteratingCol);
                wordMultiplier *= board.GetWordMultiplier(iteratingRow, iteratingCol);
            }

            wordScore += letterScore;
        }

        return wordScore * wordMultiplier;
    }
}