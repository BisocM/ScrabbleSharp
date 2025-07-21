using ScrabbleSharp.Engine.Core.Boards;
using ScrabbleSharp.Engine.Core.Models;

namespace ScrabbleSharp.Engine.Core.Rules.Interfaces;

/// <summary>
///     Defines the contract for game rules, including scoring logic and tile placement effects.
/// </summary>
public interface IGameRules
{
    /// <summary>
    ///     Gets the score for a single letter, considering its context on the board.
    /// </summary>
    /// <param name="board">The current game board.</param>
    /// <param name="row">The row of the letter.</param>
    /// <param name="col">The column of the letter.</param>
    /// <param name="letter">The letter being scored.</param>
    /// <param name="isNewTile">Whether the tile was just placed in the current move.</param>
    /// <param name="blank">Whether the tile is a blank.</param>
    /// <returns>The calculated score for the letter.</returns>
    int GetLetterScore(Board board,
        int row,
        int col,
        char letter,
        bool isNewTile,
        bool blank);

    /// <summary>
    ///     A callback method invoked when a tile is placed on the board.
    ///     This is used to apply side effects, such as consuming multipliers.
    /// </summary>
    /// <param name="board">The current game board.</param>
    /// <param name="row">The row where the tile was placed.</param>
    /// <param name="col">The column where the tile was placed.</param>
    /// <param name="letter">The letter placed.</param>
    /// <param name="blank">Whether the tile is a blank.</param>
    void OnTilePlaced(Board board,
        int row,
        int col,
        char letter,
        bool blank);

    /// <summary>
    ///     Calculates the total score for a given move, including the main word, cross words, and bonuses.
    /// </summary>
    /// <param name="board">The game board before the move is applied.</param>
    /// <param name="move">The move to be scored.</param>
    /// <returns>The total score for the move.</returns>
    int CalculateMoveScore(Board board, Move move);

    /// <summary>
    ///     Applies any final bonuses to a move's score, such as the "bingo" bonus for using all tiles.
    /// </summary>
    /// <param name="preBonusScore">The score before applying final bonuses.</param>
    /// <param name="tilesPlacedCount">The number of new tiles placed in the move.</param>
    /// <returns>The final score after applying bonuses.</returns>
    int ApplyFinalBonuses(int preBonusScore, int tilesPlacedCount);

    /// <summary>
    ///     Gets the base, unmodified score for a letter.
    /// </summary>
    /// <param name="letter">The letter (A-Z).</param>
    /// <returns>The base point value of the letter.</returns>
    int GetBaseLetterScore(char letter);
}