using ScrabbleSharp.Engine.Core.Boards;
using ScrabbleSharp.Engine.Core.Models;
using ScrabbleSharp.Engine.Core.Rules.Interfaces;
using ScrabbleSharp.Engine.Core.Tiles;

namespace ScrabbleSharp.Engine.Core.Rules.Types;

/// <summary>
///     Provides a base implementation for game rules, handling common scoring logic.
/// </summary>
public abstract class GameRulesBase : IGameRules
{
    /// <inheritdoc />
    public abstract int GetBaseLetterScore(char letter);

    /// <inheritdoc />
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
            :
            // Always return the base score for any non-blank letter.
            // The multipliers for the current move's new tiles are handled elsewhere.
            GetBaseLetterScore(letter);
    }

    /// <inheritdoc />
    public virtual void OnTilePlaced(
        Board board,
        int row,
        int col,
        char letter,
        bool blank)
    {
        var square = board.GetSquare(row, col);

        var effectiveScore = blank ? 0 : GetBaseLetterScore(letter);

        // Apply the letter multiplier from the square and store it permanently.
        effectiveScore *= square.LetterMultiplier;
        square.PermanentLetterScore = effectiveScore;

        // Consume the multiplier so it cannot be used again.
        square.SetMultiplier(MultiplierType.None);
    }

    /// <inheritdoc />
    public virtual int CalculateMoveScore(Board board, Move move)
    {
        var newTiles = move.Tiles.ToDictionary(tile => (tile.Row, tile.Col));
        var mainWordScore = 0;
        var wordMultiplier = 1;

        var (deltaRow, deltaCol) = move.IsHorizontal ? (0, 1) : (1, 0);

        // Calculate the score for the main word.
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
                letterScore *= board.GetLetterMultiplier(row, column);
                wordMultiplier *= board.GetWordMultiplier(row, column);
            }

            mainWordScore += letterScore;
        }

        mainWordScore *= wordMultiplier;

        // Calculate the score for all cross-words (words formed perpendicularly).
        var crossScore = 0;
        foreach (var tile in move.Tiles)
            crossScore += CalculateCrossWordScore(board, tile, move.IsHorizontal);

        var preBonusScore = mainWordScore + crossScore;

        return ApplyFinalBonuses(preBonusScore, move.Tiles.Count);
    }

    /// <inheritdoc />
    public virtual int ApplyFinalBonuses(int preBonusScore, int tilesPlacedCount)
    {
        // Standard Scrabble "bingo" bonus for using all 7 tiles.
        if (tilesPlacedCount == 7) return preBonusScore + 50;
        return preBonusScore;
    }

    /// <summary>
    ///     Calculates the score of a single cross-word created by a new tile placement.
    /// </summary>
    /// <param name="board">The game board.</param>
    /// <param name="placedTile">The newly placed tile that forms the cross-word.</param>
    /// <param name="mainIsHorizontal">Whether the main move was horizontal.</param>
    /// <returns>The score of the cross-word, or 0 if no cross-word was formed.</returns>
    protected virtual int CalculateCrossWordScore(Board board,
        TilePlacement placedTile,
        bool mainIsHorizontal)
    {
        var (deltaRow, deltaCol) = mainIsHorizontal ? (1, 0) : (0, 1); // Perpendicular direction
        int currentRow = placedTile.Row, currentCol = placedTile.Col;

        // Find the start of the cross-word.
        while (currentRow - deltaRow >= 0 && currentCol - deltaCol >= 0 &&
               !board.IsEmpty(currentRow - deltaRow, currentCol - deltaCol))
        {
            currentRow -= deltaRow;
            currentCol -= deltaCol;
        }

        // If the tile has no perpendicular neighbors, it didn't form a cross-word.
        if (currentRow == placedTile.Row && currentCol == placedTile.Col &&
            (currentRow + deltaRow >= board.Rows || currentCol + deltaCol >= board.Cols ||
             board.IsEmpty(currentRow + deltaRow, currentCol + deltaCol)))
            return 0;

        int wordScore = 0, wordMultiplier = 1;

        // Iterate through the letters of the cross-word to calculate its score.
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
                letterScore *= board.GetLetterMultiplier(iteratingRow, iteratingCol);
                wordMultiplier *= board.GetWordMultiplier(iteratingRow, iteratingCol);
            }

            wordScore += letterScore;
        }

        return wordScore * wordMultiplier;
    }
}