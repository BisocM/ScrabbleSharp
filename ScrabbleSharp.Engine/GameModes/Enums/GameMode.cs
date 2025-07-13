namespace ScrabbleSharp.Engine.GameModes.Enums;

/// <summary>
///     Enumerates the supported game modes.
/// </summary>
public enum GameMode
{
    /// <summary>
    ///     Classic Letter League mode.
    /// </summary>
    LetterLeagueClassic = 0,

    /// <summary>
    ///     Classic Scrabble mode.
    /// </summary>
    ScrabbleClassic = 1,

    /// <summary>
    ///     Super Scrabble mode with a 21x21 board.
    /// </summary>
    ScrabbleSuper = 2,

    /// <summary>
    ///     Scrabble Duel mode with an 11x11 board.
    /// </summary>
    ScrabbleDuel = 3
}