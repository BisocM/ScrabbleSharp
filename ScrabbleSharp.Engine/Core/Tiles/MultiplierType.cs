namespace ScrabbleSharp.Engine.Core.Tiles;

/// <summary>
///     Represents the type of score multiplier on a board square.
/// </summary>
public enum MultiplierType
{
    /// <summary>
    ///     No multiplier.
    /// </summary>
    None,

    /// <summary>
    ///     Doubles the score of the letter placed on this square.
    /// </summary>
    DoubleLetter,

    /// <summary>
    ///     Triples the score of the letter placed on this square.
    /// </summary>
    TripleLetter,

    /// <summary>
    ///     Quadruples the score of the letter placed on this square.
    /// </summary>
    QuadrupleLetter,

    /// <summary>
    ///     Doubles the score of the entire word containing this square.
    /// </summary>
    DoubleWord,

    /// <summary>
    ///     Triples the score of the entire word containing this square.
    /// </summary>
    TripleWord,

    /// <summary>
    ///     Quadruples the score of the entire word containing this square.
    /// </summary>
    QuadrupleWord
}