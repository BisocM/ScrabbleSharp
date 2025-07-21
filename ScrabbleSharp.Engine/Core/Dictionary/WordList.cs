namespace ScrabbleSharp.Engine.Core.Dictionary;

/// <summary>
///     Enumerates the available word lists for different Scrabble lexicons.
/// </summary>
public enum WordList
{
    /// <summary>
    ///     SOWPODS/Collins Scrabble Words (CSW), used in most English-speaking countries except the US, Canada, and Thailand.
    /// </summary>
    Sowpods,

    /// <summary>
    ///     Tournament Word List (TWL), used in the US, Canada, and Thailand.
    /// </summary>
    Twl,

    /// <summary>
    ///     Collins Official Scrabble Words 2019 edition.
    /// </summary>
    Collins19,

    /// <summary>
    ///     A large, comprehensive word list derived from Wordnik, used for Letter League.
    /// </summary>
    Wordnik
}