using System.Collections.Immutable;

namespace ScrabbleSharp.Engine.Core.Models;

/// <summary>
///     Represents a player's move, including the word, position, tiles used, and score.
/// </summary>
public sealed record Move
{
    /// <summary>
    ///     Gets the full word formed by the move.
    /// </summary>
    public string Word { get; init; } = string.Empty;

    /// <summary>
    ///     Gets the starting row of the word.
    /// </summary>
    public int StartRow { get; init; }

    /// <summary>
    ///     Gets the starting column of the word.
    /// </summary>
    public int StartCol { get; init; }

    /// <summary>
    ///     Gets a value indicating whether the word is placed horizontally.
    /// </summary>
    public bool IsHorizontal { get; init; }

    /// <summary>
    ///     Gets the list of new tiles placed on the board for this move.
    /// </summary>
    public ImmutableList<TilePlacement> Tiles { get; init; }
        = ImmutableList<TilePlacement>.Empty;

    /// <summary>
    ///     Gets the total score of the move.
    /// </summary>
    public int Score { get; init; }
    
    /// <summary>
    ///     Gets the definition of the word.
    /// </summary>
    public string Defintion { get; set; } = string.Empty;
}