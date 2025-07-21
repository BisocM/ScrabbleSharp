using ScrabbleSharp.Contracts.Protos;

namespace ScrabbleSharp.Engine.Core.Boards.Interfaces;

/// <summary>
///     Defines the contract for a board layout that can be dynamically expanded.
/// </summary>
public interface IExpandableBoardLayout : IBoardLayout
{
    /// <summary>
    ///     Attempts to expand the board at a given coordinate, typically near an edge.
    /// </summary>
    /// <param name="row">The row index triggering the expansion attempt.</param>
    /// <param name="column">The column index triggering the expansion attempt.</param>
    /// <returns>An <see cref="ExpandDelta" /> object if expansion was successful, or <c>null</c> if it failed (e.g., limit reached).</returns>
    ExpandDelta? TryExpandAt(int row, int column);

    /// <summary>
    ///     Resets the layout to its initial, non-expanded state.
    /// </summary>
    void Reset();

    /// <summary>
    ///     Sets the maximum number of expansion bands allowed in each direction.
    /// </summary>
    /// <param name="up">The maximum number of upward expansions.</param>
    /// <param name="down">The maximum number of downward expansions.</param>
    /// <param name="left">The maximum number of leftward expansions.</param>
    /// <param name="right">The maximum number of rightward expansions.</param>
    void SetExpansionLimits(
        int? up = null,
        int? down = null,
        int? left = null,
        int? right = null);
}