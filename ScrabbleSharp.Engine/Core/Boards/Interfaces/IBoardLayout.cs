using ScrabbleSharp.Engine.Core.Tiles;

namespace ScrabbleSharp.Engine.Core.Boards.Interfaces;

/// <summary>
///     Defines the structure of a Scrabble board, including its dimensions and multiplier placements.
/// </summary>
public interface IBoardLayout
{
    /// <summary>
    ///     Gets the number of rows on the board.
    /// </summary>
    int Rows { get; }

    /// <summary>
    ///     Gets the number of columns on the board.
    /// </summary>
    int Cols { get; }

    /// <summary>
    ///     Zero-based coordinates of the permanent start square (★).
    ///     Implementations must guarantee these never change for the lifetime of the layout
    ///     (even after any successful calls to <c>TryExpandAt</c>).
    /// </summary>
    int OriginRow { get; }

    /// <summary>
    ///     Zero-based coordinates of the permanent start square (★).
    ///     Implementations must guarantee these never change for the lifetime of the layout
    ///     (even after any successful calls to <c>TryExpandAt</c>).
    /// </summary>
    int OriginCol { get; }

    /// <summary>
    ///     Gets the multiplier type for a specific square.
    /// </summary>
    /// <param name="row">The row index of the square.</param>
    /// <param name="column">The column index of the square.</param>
    /// <returns>The <see cref="MultiplierType" /> at the specified location.</returns>
    MultiplierType GetMultiplier(int row, int column);
}