using ScrabbleSharp.Contracts.Protos;
using ScrabbleSharp.Engine.GameModes.Enums;
using Move = ScrabbleSharp.Engine.Core.Models.Move;

namespace ScrabbleSharp.Gateway.Services;

/// <summary>
/// Defines the contract for the core game service that handles game logic operations.
/// </summary>
public interface IGameService
{
    /// <summary>
    /// Asynchronously solves for the best moves given a board state and rack.
    /// </summary>
    /// <param name="rack">The player's rack of letters.</param>
    /// <param name="board">The string representation of the board.</param>
    /// <param name="mode">The game mode being played.</param>
    /// <param name="bands">The current expansion state for expandable boards.</param>
    /// <param name="cancellationToken">A token for cancelling the operation.</param>
    /// <returns>A task that represents the asynchronous operation, containing an enumerable of found moves.</returns>
    Task<IEnumerable<Move>> SolveAsync(
        string rack,
        string board,
        GameMode mode,
        (int Up, int Down, int Left, int Right) bands,
        CancellationToken cancellationToken);

    /// <summary>
    /// Gets the board layout for a specified game mode.
    /// </summary>
    /// <param name="mode">The game mode.</param>
    /// <returns>A <see cref="LayoutResponse"/> describing the board layout.</returns>
    LayoutResponse GetLayout(GameMode mode);

    /// <summary>
    /// Expands an expandable board layout in a given direction.
    /// </summary>
    /// <param name="mode">The game mode.</param>
    /// <param name="bands">The current expansion state of the board.</param>
    /// <param name="direction">The direction to expand in.</param>
    /// <returns>An <see cref="ExpandDelta"/> describing the changes to the layout.</returns>
    ExpandDelta ExpandLayout(
        GameMode mode,
        (int Up, int Down, int Left, int Right) bands,
        Direction direction);
}