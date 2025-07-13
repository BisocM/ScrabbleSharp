namespace ScrabbleSharp.Engine.Core.Dictionary;

/// <summary>
///     Enumerates the available word lists for different Scrabble standards.
/// </summary>
public enum WordList
{
    /// <summary>
    ///     International word list (SOWPODS/CSW), words only.
    /// </summary>
    Sowpods,

    /// <summary>
    ///     North American list (NASPA/NWL), words only.
    /// </summary>
    Twl,

    /// <summary>
    ///     Collins 2019 dictionary, with definitions.
    /// </summary>
    Collins19,

    /// <summary>
    ///     Wordnik public word list, with definitions.
    /// </summary>
    Wordnik
}