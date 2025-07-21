using ScrabbleSharp.Engine.Core.Tiles;

namespace ScrabbleSharp.Engine.Core.Boards.Interfaces;

/// <summary>
///     Defines the contract for a board layout, specifying its dimensions, origin, and multiplier distribution.
/// </summary>
public interface IBoardLayout
{
    /// <summary>
    ///     Gets the total number of rows in the layout.
    /// </summary>
    int Rows { get; }

    /// <summary>
    ///     Gets the total number of columns in the layout.
    /// </summary>
    int Cols { get; }

    /// <summary>
    ///     Gets the row index of the board's origin (center square).
    /// </summary>
    int OriginRow { get; }

    /// <summary>
    ///     Gets the column index of the board's origin (center square).
    /// </summary>
    int OriginCol { get; }

    /// <summary>
    ///     Gets the multiplier type for a specific square in the layout.
    /// </summary>
    /// <param name="row">The row index of the square.</param>
    /// <param name="column">The column index of the square.</param>
    /// <returns>The <see cref="MultiplierType" /> at the specified coordinates.</returns>
    MultiplierType GetMultiplier(int row, int column);
}