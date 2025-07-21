namespace ScrabbleSharp.Engine.Core.Tiles;

/// <summary>
///     Represents a single square on the game board.
/// </summary>
public sealed class Square
{
    /// <summary>
    ///     Gets or sets the letter tile placed on this square. A <c>null</c> value indicates the square is empty.
    /// </summary>
    public char? Letter { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the tile on this square is a blank.
    /// </summary>
    public bool IsBlank { get; set; }

    /// <summary>
    ///     Gets or sets the score contribution of a letter placed on this square, after any letter multipliers have been applied.
    ///     This value is calculated when a tile is placed and is not currently used for subsequent score calculations.
    /// </summary>
    public int? PermanentLetterScore { get; internal set; }

    /// <summary>
    ///     Gets the multiplier type currently active on this square.
    /// </summary>
    public MultiplierType Multiplier { get; private set; } = MultiplierType.None;

    /// <summary>
    ///     Gets the numerical value of the letter multiplier for this square.
    /// </summary>
    /// <returns>2, 3, or 4 for letter multipliers; otherwise, 1.</returns>
    public int LetterMultiplier =>
        Multiplier switch
        {
            MultiplierType.DoubleLetter => 2,
            MultiplierType.TripleLetter => 3,
            MultiplierType.QuadrupleLetter => 4,
            _ => 1
        };

    /// <summary>
    ///     Gets the numerical value of the word multiplier for this square.
    /// </summary>
    /// <returns>2, 3, or 4 for word multipliers; otherwise, 1.</returns>
    public int WordMultiplier =>
        Multiplier switch
        {
            MultiplierType.DoubleWord => 2,
            MultiplierType.TripleWord => 3,
            MultiplierType.QuadrupleWord => 4,
            _ => 1
        };

    /// <summary>
    ///     Sets the multiplier for this square. This is intended for internal use during board initialization.
    /// </summary>
    /// <param name="multiplier">The multiplier type to set.</param>
    internal void SetMultiplier(MultiplierType multiplier) => Multiplier = multiplier;
}