namespace ScrabbleSharp.Engine.Core.Tiles;

/// <summary>
///     Enumerates the types of score multipliers that can be on a board square.
/// </summary>
public enum MultiplierType
{
    /// <summary>
    ///     No multiplier.
    /// </summary>
    None,

    /// <summary>
    ///     Doubles the value of the letter placed on this square.
    /// </summary>
    DoubleLetter,

    /// <summary>
    ///     Triples the value of the letter placed on this square.
    /// </summary>
    TripleLetter,

    /// <summary>
    ///     Quadruples the value of the letter placed on this square.
    /// </summary>
    QuadrupleLetter,

    /// <summary>
    ///     Doubles the value of the entire word.
    /// </summary>
    DoubleWord,

    /// <summary>
    ///     Triples the value of the entire word.
    /// </summary>
    TripleWord,

    /// <summary>
    ///     Quadruples the value of the entire word.
    /// </summary>
    QuadrupleWord
}