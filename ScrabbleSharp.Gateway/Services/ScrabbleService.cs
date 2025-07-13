using Grpc.Core;
using ScrabbleSharp.Contracts.Protos;
using ScrabbleSharp.Gateway.Extensions;

namespace ScrabbleSharp.Gateway.Services;

public sealed class ScrabbleService(IGameService gameService, ILogger<ScrabbleService> logger)
    : ScrabbleSolver.ScrabbleSolverBase
{
    public override async Task<SolveResponse> Solve(SolveRequest request, ServerCallContext context)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            var gameMode = context.ParseGameMode();
            var bands = context.ParseBandHeaders();

            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(context.CancellationToken, timeoutCts.Token);

            var moves = await gameService.SolveAsync(request.Rack, request.Board, gameMode, bands, linkedCts.Token);

            var response = new SolveResponse();
            foreach (var move in moves)
                response.Moves.Add(move.ToProto());

            stopwatch.Stop();
            logger.LogInformation("Request completed for rack {Rack} in {Duration}ms.", request.Rack, stopwatch.ElapsedMilliseconds);
            return response;
        }
        catch (ArgumentException ex)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
        }
        catch (OperationCanceledException)
        {
            stopwatch.Stop();
            if (context.CancellationToken.IsCancellationRequested)
            {
                logger.LogWarning("Solve canceled by client after {Duration}ms", stopwatch.ElapsedMilliseconds);
                throw new RpcException(new Status(StatusCode.Cancelled, "Operation canceled by the client."));
            }
            logger.LogWarning("Solve timed out after {Duration}ms for rack {Rack}", stopwatch.ElapsedMilliseconds, request.Rack);
            throw new RpcException(new Status(StatusCode.DeadlineExceeded, "The request took too long to process."));
        }
        catch (RpcException)
        {
            // Re-throw known RpcExceptions from the service layer
            throw;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            logger.LogError(ex, "Solve failed after {Duration}ms for rack {Rack}", stopwatch.ElapsedMilliseconds, request.Rack);
            throw new RpcException(new Status(StatusCode.Internal, "An internal error occurred during move generation."));
        }
    }

    public override Task<LayoutResponse> GetLayout(LayoutRequest request, ServerCallContext context)
    {
        var gameMode = context.ParseGameMode();
        var response = gameService.GetLayout(gameMode);
        return Task.FromResult(response);
    }

    public override Task<ExpandDelta> Expand(ExpandRequest request, ServerCallContext context)
    {
        var mode = context.ParseGameMode();
        var bands = context.ParseBandHeaders();
        var delta = gameService.ExpandLayout(mode, bands, request.Dir);
        return Task.FromResult(delta);
    }
}