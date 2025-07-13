using ScrabbleSharp.Contracts.Protos;

namespace ScrabbleSharp.Engine.Core.Boards.Interfaces;

/// <summary>
///     Represents a board layout that can be dynamically expanded during gameplay.
/// </summary>
public interface IExpandableBoardLayout : IBoardLayout
{
    /// <summary>
    ///     Attempts to expand the board at a given coordinate, if it's near an edge.
    /// </summary>
    /// <param name="row">The row index of the potential expansion point.</param>
    /// <param name="column">The column index of the potential expansion point.</param>
    /// <returns>An <see cref="ExpandDelta" /> object if expansion occurred; otherwise, null.</returns>
    ExpandDelta? TryExpandAt(int row, int column);

    /// <summary>
    ///     Resets the board layout to its initial state, removing all expansions.
    /// </summary>
    void Reset();
    
    /// <summary>
    /// Globally limits the number of expansion “bands” that can be added to
    /// each edge of the board.  Pass <c>null</c> to leave the current limit
    /// unchanged.
    /// </summary>
    void SetExpansionLimits(
        int? up    = null,
        int? down  = null,
        int? left  = null,
        int? right = null);
}