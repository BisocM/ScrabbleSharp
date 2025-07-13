using System.Diagnostics;
using Grpc.Core;
using Microsoft.Extensions.Options;
using ScrabbleSharp.Contracts.Protos;
using ScrabbleSharp.Engine.Core.Boards.Interfaces;
using ScrabbleSharp.Engine.GameModes;
using ScrabbleSharp.Engine.GameModes.Enums;
using ScrabbleSharp.Engine.Serialization;
using ScrabbleSharp.Engine.Services.MoveGeneration;
using ScrabbleSharp.Gateway.Configuration;
using ScrabbleSharp.Gateway.Extensions;
using EngineMove = ScrabbleSharp.Engine.Core.Models.Move;

namespace ScrabbleSharp.Gateway.Services;

public sealed class GameService(
    GameModeRegistry gameModeRegistry,
    IServiceProvider serviceProvider,
    MoveGenerator moveGenerator,
    IOptions<ValidationSettings> validationSettings,
    ILogger<GameService> logger)
    : IGameService
{
    private readonly ValidationSettings _validationSettings = validationSettings.Value;

    public async Task<IEnumerable<EngineMove>> SolveAsync(
        string rack,
        string boardString,
        GameMode mode,
        (int Up, int Down, int Left, int Right) bands,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var gameModeProvider = gameModeRegistry.Get(mode);
        var layout = BuildLayout(mode, bands);
        var trie = gameModeRegistry.GetTrie(gameModeProvider.Dictionary);

        // Semantic validation: ensure the provided board fits the generated layout.
        var lines = boardString.Split('\n');
        if (lines.Length > layout.Rows)
            throw new ArgumentException($"Submitted board has {lines.Length} rows but the selected layout supports only {layout.Rows}.");
        if (lines.Any(l => l.Length > layout.Cols))
            throw new ArgumentException($"Submitted board has a row longer than the layout's {layout.Cols} columns.");

        var board = BoardSnapshot.FromString(boardString, layout, gameModeProvider.Rules);

        var allMoves = await Task.Run(
            () => moveGenerator.GenerateAllMoves(rack, board, trie, gameModeProvider.Rules),
            cancellationToken);

        var balancedMoves = BalanceMoveResults(allMoves);

        stopwatch.Stop();
        logger.LogInformation(
            "Solved rack {Rack} (mode={Mode}, bands={@Bands}): found {Count} moves in {Duration}ms",
            rack, mode, bands, balancedMoves.Count, stopwatch.ElapsedMilliseconds);

        // Attach definitions to moves
        foreach (var move in balancedMoves)
            move.Defintion = trie.GetDefinition(move.Word) ?? string.Empty;

        return balancedMoves.OrderByDescending(m => m.Score);
    }

    public LayoutResponse GetLayout(GameMode mode)
    {
        var layout = gameModeRegistry.Get(mode).CreateLayout(serviceProvider);
        return layout.ToLayoutResponse();
    }

    public ExpandDelta ExpandLayout(
        GameMode mode,
        (int Up, int Down, int Left, int Right) bands,
        Direction direction)
    {
        var provider = gameModeRegistry.Get(mode);
        var layout = provider.CreateLayout(serviceProvider);

        if (layout is not IExpandableBoardLayout expandable)
            throw new RpcException(new Status(StatusCode.Unimplemented, "This game mode's layout cannot be expanded."));

        if (direction switch
            {
                Direction.Up => bands.Up >= _validationSettings.MaxBandsPerDirection,
                Direction.Down => bands.Down >= _validationSettings.MaxBandsPerDirection,
                Direction.Left => bands.Left >= _validationSettings.MaxBandsPerDirection,
                Direction.Right => bands.Right >= _validationSettings.MaxBandsPerDirection,
                _ => false
            })
        {
            throw new RpcException(new Status(StatusCode.FailedPrecondition, "Expansion limit has been reached for this direction."));
        }
        
        expandable.ApplyBands(bands);

        var (triggerRow, triggerCol) = expandable.GetTriggerCoordinates(direction);
        var delta = expandable.TryExpandAt(triggerRow, triggerCol);

        if (delta is null)
            throw new RpcException(new Status(StatusCode.FailedPrecondition, "Expansion failed. The limit may have been reached."));

        if (delta.TotalRows > _validationSettings.MaxBoardDimension || delta.TotalCols > _validationSettings.MaxBoardDimension)
        {
            throw new RpcException(new Status(StatusCode.FailedPrecondition,
                $"Expansion would exceed the maximum board size of {_validationSettings.MaxBoardDimension}×{_validationSettings.MaxBoardDimension}."));
        }

        logger.LogInformation("Expanded {Dir} (mode={Mode}, prevBands={@Prev}, newSize={{R:{Rows},C:{Cols}}})",
            direction, mode, bands, delta.TotalRows, delta.TotalCols);
            
        return delta;
    }

    private IBoardLayout BuildLayout(GameMode mode, (int Up, int Down, int Left, int Right) bands)
    {
        var provider = gameModeRegistry.Get(mode);
        var layout = provider.CreateLayout(serviceProvider);
        if (layout is IExpandableBoardLayout expandable)
            expandable.ApplyBands(bands);
        return layout;
    }

    private List<EngineMove> BalanceMoveResults(IReadOnlyList<EngineMove> moves)
    {
        const int maxMovesToReturn = 128;
        var quotas = new Dictionary<int, int>
        {
            [7] = 40, [6] = 30, [5] = 20,
            [4] = 15, [3] = 15, [2] = 8
        };

        var balancedMoves = new List<EngineMove>();
        var movesByLength = moves.GroupBy(m => m.Word.Length).OrderByDescending(g => g.Key);

        foreach (var group in movesByLength)
        {
            if (balancedMoves.Count >= maxMovesToReturn) break;
            if (!quotas.TryGetValue(group.Key, out var quota)) continue;
            
            var remainingSlots = maxMovesToReturn - balancedMoves.Count;
            var takeCount = Math.Min(quota, remainingSlots);
            balancedMoves.AddRange(group.OrderByDescending(m => m.Score).Take(takeCount));
        }

        return balancedMoves;
    }
}