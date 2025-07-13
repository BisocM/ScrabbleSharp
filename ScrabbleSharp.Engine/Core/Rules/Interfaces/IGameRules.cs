using ScrabbleSharp.Engine.Core.Boards;
using ScrabbleSharp.Engine.Core.Models;

namespace ScrabbleSharp.Engine.Core.Rules.Interfaces;

/// <summary>
///     Defines the rules for scoring and gameplay mechanics.
/// </summary>
public interface IGameRules
{
    /// <summary>
    ///     Gets the score for a single letter on a given square.
    /// </summary>
    /// <param name="board">The game board.</param>
    /// <param name="row">The row of the letter.</param>
    /// <param name="col">The column of the letter.</param>
    /// <param name="letter">The letter character.</param>
    /// <param name="isNewTile">Indicates if the tile is being placed in the current move.</param>
    /// <param name="blank">Indicates if the tile is a blank.</param>
    /// <returns>The calculated score for the letter.</returns>
    int GetLetterScore(Board board,
        int row,
        int col,
        char letter,
        bool isNewTile,
        bool blank);

    /// <summary>
    ///     A callback executed when a tile is placed on the board.
    ///     Used to update square state, such as consuming multipliers.
    /// </summary>
    /// <param name="board">The game board.</param>
    /// <param name="row">The row where the tile was placed.</param>
    /// <param name="col">The column where the tile was placed.</param>
    /// <param name="letter">The letter character.</param>
    /// <param name="blank">Indicates if the tile was a blank.</param>
    void OnTilePlaced(Board board,
        int row,
        int col,
        char letter,
        bool blank);

    /// <summary>
    ///     Calculates the total score for a given move, including main word, cross words, and bonuses.
    /// </summary>
    /// <param name="board">The game board.</param>
    /// <param name="move">The move to score.</param>
    /// <returns>The total score for the move.</returns>
    int CalculateMoveScore(Board board, Move move);

    /// <summary>
    ///     Applies final bonuses to a move's score, such as the bingo bonus.
    /// </summary>
    /// <param name="preBonusScore">The score before any final bonuses.</param>
    /// <param name="tilesPlacedCount">The number of tiles placed in the move.</param>
    /// <returns>The final score including bonuses.</returns>
    int ApplyFinalBonuses(int preBonusScore, int tilesPlacedCount);

    /// <summary>
    ///     Gets the base score for a letter, irrespective of its position on the board.
    /// </summary>
    /// <param name="letter">The letter character.</param>
    /// <returns>The base point value of the letter.</returns>
    int GetBaseLetterScore(char letter);
}