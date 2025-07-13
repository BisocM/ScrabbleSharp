namespace ScrabbleSharp.Engine.Core.Tiles;

/// <summary>
///     Represents a single square on the game board.
/// </summary>
public sealed class Square
{
    /// <summary>
    ///     Gets or sets the letter placed on this square. Null if empty.
    /// </summary>
    public char? Letter { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the placed letter is from a blank tile.
    /// </summary>
    public bool IsBlank { get; set; }

    /// <summary>
    ///     Gets or sets the letter score after any initial multiplier has been applied.
    ///     This score is used for subsequent cross-word calculations.
    /// </summary>
    public int? PermanentLetterScore { get; internal set; }

    /// <summary>
    ///     Gets the score multiplier associated with this square.
    /// </summary>
    public MultiplierType Multiplier { get; private set; } = MultiplierType.None;

    /// <summary>
    ///     Gets the numerical value of the letter multiplier (e.g., 2 for DoubleLetter).
    /// </summary>
    public int LetterMultiplier =>
        Multiplier switch
        {
            MultiplierType.DoubleLetter => 2,
            MultiplierType.TripleLetter => 3,
            MultiplierType.QuadrupleLetter => 4,
            _ => 1
        };

    /// <summary>
    ///     Gets the numerical value of the word multiplier (e.g., 2 for DoubleWord).
    /// </summary>
    public int WordMultiplier =>
        Multiplier switch
        {
            MultiplierType.DoubleWord => 2,
            MultiplierType.TripleWord => 3,
            MultiplierType.QuadrupleWord => 4,
            _ => 1
        };

    /// <summary>
    ///     Sets the multiplier for this square. This is an internal method called during board setup
    ///     and after a multiplier is used.
    /// </summary>
    internal void SetMultiplier(MultiplierType multiplier) => Multiplier = multiplier;
}