using ScrabbleSharp.Engine.Core.Boards;

namespace ScrabbleSharp.Engine.Core.Rules.Interfaces;

/// <summary>
///     Defines a contract for components that perform additional initialization on a board
///     after it has been constructed, typically after loading from a snapshot.
/// </summary>
public interface IBoardInitialiser
{
    /// <summary>
    ///     Performs initialization logic on the provided board.
    /// </summary>
    /// <param name="board">The board to initialize.</param>
    void Initialise(Board board);
}