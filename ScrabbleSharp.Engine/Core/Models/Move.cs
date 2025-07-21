using System.Collections.Immutable;

namespace ScrabbleSharp.Engine.Core.Models;

/// <summary>
///     Represents a single move in the game, including the word played, its position, score, and the tiles used.
/// </summary>
public sealed record Move
{
    /// <summary>
    ///     The primary word formed by the move.
    /// </summary>
    public string Word { get; init; } = string.Empty;

    /// <summary>
    ///     The starting row index of the word.
    /// </summary>
    public int StartRow { get; init; }

    /// <summary>
    ///     The starting column index of the word.
    /// </summary>
    public int StartCol { get; init; }

    /// <summary>
    ///     Indicates if the word is played horizontally (<c>true</c>) or vertically (<c>false</c>).
    /// </summary>
    public bool IsHorizontal { get; init; }

    /// <summary>
    ///     The list of new tiles placed on the board for this move.
    /// </summary>
    public ImmutableList<TilePlacement> Tiles { get; init; }
        = ImmutableList<TilePlacement>.Empty;

    /// <summary>
    ///     The total score awarded for this move.
    /// </summary>
    public int Score { get; init; }

    /// <summary>
    ///     The dictionary definition of the word.
    /// </summary>
    public string Defintion { get; set; } = string.Empty;
}