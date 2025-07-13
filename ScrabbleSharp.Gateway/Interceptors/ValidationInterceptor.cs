using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.Options;
using ScrabbleSharp.Contracts.Protos;
using ScrabbleSharp.Gateway.Configuration;

namespace ScrabbleSharp.Gateway.Interceptors;

public sealed class ValidationInterceptor(IOptions<ValidationSettings> settings) : Interceptor
{
    private readonly ValidationSettings _settings = settings.Value;

    public override Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        if (request is SolveRequest solveRequest)
            ValidateSolveRequest(solveRequest);
            
        return continuation(request, context);
    }

    private void ValidateSolveRequest(SolveRequest solveRequest)
    {
        // Rack validation
        if (solveRequest.Rack.Length > _settings.MaxRackSize)
            throw new RpcException(new Status(StatusCode.InvalidArgument, $"Rack size cannot exceed {_settings.MaxRackSize} tiles."));

        foreach (var ch in solveRequest.Rack.Where(ch => !IsValidRackChar(ch)))
            throw new RpcException(new Status(StatusCode.InvalidArgument, $"Invalid character '{ch}' in rack."));

        var wildcardCount = solveRequest.Rack.Count(c => c is '*' or '_');
        if (wildcardCount > _settings.MaxWildcards)
            throw new RpcException(new Status(StatusCode.InvalidArgument, $"No more than {_settings.MaxWildcards} wildcards are allowed."));
        
        // Board syntactic validation
        var lines = solveRequest.Board.Split('\n');
        if (lines.Length > _settings.MaxBoardDimension)
            throw new RpcException(new Status(StatusCode.InvalidArgument, $"Board dimensions cannot exceed {_settings.MaxBoardDimension} rows."));

        if (lines.Any(l => l.Length > _settings.MaxBoardDimension))
            throw new RpcException(new Status(StatusCode.InvalidArgument, $"Board dimensions cannot exceed {_settings.MaxBoardDimension} columns."));

        for (var row = 0; row < lines.Length; row++)
        {
            foreach (var ch in lines[row].Where(ch => !IsValidBoardChar(ch)))
                throw new RpcException(new Status(StatusCode.InvalidArgument, $"Invalid character '{ch}' on board at row {row + 1}."));
        }
    }

    private static bool IsValidRackChar(char ch) => ch is >= 'A' and <= 'Z' or >= 'a' and <= 'z' or '*' or '_';
    private static bool IsValidBoardChar(char ch) => ch is '.' or ' ' or '0' or >= 'A' and <= 'Z' or >= 'a' and <= 'z';
}