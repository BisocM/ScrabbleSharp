using ScrabbleSharp.Engine.Core.Boards;

namespace ScrabbleSharp.Engine.Core.Rules.Interfaces;

/// <summary>
///     Defines a contract for logic that initializes a board after it has been constructed,
///     for example, by applying a snapshot of a game in progress.
/// </summary>
public interface IBoardInitialiser
{
    /// <summary>
    ///     Initializes the specified board.
    /// </summary>
    /// <param name="board">The board to initialize.</param>
    void Initialise(Board board);
}