namespace ScrabbleSharp.Engine.GameModes.Enums;

/// <summary>
///     Enumerates the supported game modes.
/// </summary>
public enum GameMode
{
    /// <summary>
    ///     Letter League Classic mode, featuring an expandable board and a large dictionary.
    /// </summary>
    LetterLeagueClassic = 0,

    /// <summary>
    ///     Standard classic Scrabble on a 15x15 board.
    /// </summary>
    ScrabbleClassic = 1,

    /// <summary>
    ///     Super Scrabble on a 21x21 board.
    /// </summary>
    ScrabbleSuper = 2,

    /// <summary>
    ///     Scrabble Duel on an 11x11 board.
    /// </summary>
    ScrabbleDuel = 3
}