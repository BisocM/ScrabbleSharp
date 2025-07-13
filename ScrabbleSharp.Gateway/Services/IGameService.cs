using ScrabbleSharp.Contracts.Protos;
using ScrabbleSharp.Engine.GameModes.Enums;
using Move = ScrabbleSharp.Engine.Core.Models.Move;

namespace ScrabbleSharp.Gateway.Services;

public interface IGameService
{
    Task<IEnumerable<Move>> SolveAsync(
        string rack,
        string board,
        GameMode mode,
        (int Up, int Down, int Left, int Right) bands,
        CancellationToken cancellationToken);

    LayoutResponse GetLayout(GameMode mode);

    ExpandDelta ExpandLayout(
        GameMode mode,
        (int Up, int Down, int Left, int Right) bands,
        Direction direction);
}