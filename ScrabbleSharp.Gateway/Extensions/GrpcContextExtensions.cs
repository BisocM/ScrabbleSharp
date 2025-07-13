using Grpc.Core;
using ScrabbleSharp.Engine.GameModes.Enums;

namespace ScrabbleSharp.Gateway.Extensions;

/// <summary>
///     Provides extension methods for <see cref="ServerCallContext" /> to parse custom request headers.
/// </summary>
public static class GrpcContextExtensions
{
    /// <summary>
    ///     Parses the custom 'x-up', 'x-down', 'x-left', and 'x-right' headers
    ///     to determine the current expansion state of the board.
    /// </summary>
    /// <param name="context">The gRPC server call context.</param>
    /// <returns>A tuple containing the number of bands expanded in each direction.</returns>
    public static (int Up, int Down, int Left, int Right) ParseBandHeaders(this ServerCallContext context)
    {
        return (ParseHeader("x-up"), ParseHeader("x-down"), ParseHeader("x-left"), ParseHeader("x-right"));
        int ParseHeader(string key) => int.TryParse(context.RequestHeaders.GetValue(key), out var value) ? value : 0;
    }

    /// <summary>
    ///     Parses the custom 'x-mode' header to determine the requested game mode.
    /// </summary>
    /// <param name="context">The gRPC server call context.</param>
    /// <returns>The parsed <see cref="GameMode" />, defaulting to LetterLeagueClassic.</returns>
    public static GameMode ParseGameMode(this ServerCallContext context)
    {
        var rawMode = context.RequestHeaders.GetValue("x-mode") ?? "letterleague_classic";
        return rawMode.ToLowerInvariant() switch
        {
            "letterleague_classic" => GameMode.LetterLeagueClassic,
            "scrabble_super" => GameMode.ScrabbleSuper,
            "scrabble_classic" => GameMode.ScrabbleClassic,
            "scrabble_duel" => GameMode.ScrabbleDuel,
            _ => GameMode.LetterLeagueClassic
        };
    }
}