using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.Options;
using ScrabbleSharp.Contracts.Protos;
using ScrabbleSharp.Gateway.Configuration;

namespace ScrabbleSharp.Gateway.Interceptors;

/// <summary>
///     A gRPC interceptor that validates incoming requests against predefined rules.
/// </summary>
public sealed class ValidationInterceptor(IOptions<ValidationSettings> settings) : Interceptor
{
    private readonly ValidationSettings _settings = settings.Value;

    /// <summary>
    ///     Intercepts a unary server call to perform validation before the request handler is invoked.
    /// </summary>
    /// <param name="request">The request message.</param>
    /// <param name="context">The server call context.</param>
    /// <param name="continuation">The delegate to invoke the next handler in the chain.</param>
    /// <returns>The response from the handler.</returns>
    public override Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        if (request is SolveRequest solveRequest)
            ValidateSolveRequest(solveRequest);

        return continuation(request, context);
    }

    /// <summary>
    ///     Validates the fields of a <see cref="SolveRequest" />.
    /// </summary>
    /// <param name="solveRequest">The request to validate.</param>
    /// <exception cref="RpcException">Thrown with <see cref="StatusCode.InvalidArgument" /> if validation fails.</exception>
    private void ValidateSolveRequest(SolveRequest solveRequest)
    {
        if (solveRequest.Rack.Length > _settings.MaxRackSize)
            throw new RpcException(new Status(StatusCode.InvalidArgument, $"Rack size cannot exceed {_settings.MaxRackSize} tiles."));

        foreach (var ch in solveRequest.Rack.Where(ch => !IsValidRackChar(ch)))
            throw new RpcException(new Status(StatusCode.InvalidArgument, $"Invalid character '{ch}' in rack."));

        var wildcardCount = solveRequest.Rack.Count(c => c is '*' or '_');
        if (wildcardCount > _settings.MaxWildcards)
            throw new RpcException(new Status(StatusCode.InvalidArgument, $"No more than {_settings.MaxWildcards} wildcards are allowed."));

        var lines = solveRequest.Board.Split('\n');
        if (lines.Length > _settings.MaxBoardDimension)
            throw new RpcException(new Status(StatusCode.InvalidArgument, $"Board dimensions cannot exceed {_settings.MaxBoardDimension} rows."));

        if (lines.Any(l => l.Length > _settings.MaxBoardDimension))
            throw new RpcException(new Status(StatusCode.InvalidArgument, $"Board dimensions cannot exceed {_settings.MaxBoardDimension} columns."));

        for (var row = 0; row < lines.Length; row++)
            foreach (var ch in lines[row].Where(ch => !IsValidBoardChar(ch)))
                throw new RpcException(new Status(StatusCode.InvalidArgument, $"Invalid character '{ch}' on board at row {row + 1}."));
    }

    /// <summary>
    ///     Checks if a character is valid for a rack string.
    /// </summary>
    private static bool IsValidRackChar(char ch) => ch is >= 'A' and <= 'Z' or >= 'a' and <= 'z' or '*' or '_';

    /// <summary>
    ///     Checks if a character is valid for a board snapshot string.
    /// </summary>
    private static bool IsValidBoardChar(char ch) => ch is '.' or ' ' or '0' or >= 'A' and <= 'Z' or >= 'a' and <= 'z';
}