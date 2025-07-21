using Grpc.Core;
using ScrabbleSharp.Engine.GameModes.Enums;

namespace ScrabbleSharp.Gateway.Extensions;

/// <summary>
///     Provides extension methods for <see cref="ServerCallContext" /> to simplify request metadata parsing.
/// </summary>
public static class GrpcContextExtensions
{
    /// <summary>
    ///     Parses custom headers for board expansion bands.
    /// </summary>
    /// <param name="context">The gRPC server call context.</param>
    /// <returns>A tuple (Up, Down, Left, Right) with the number of bands for each direction.</returns>
    public static (int Up, int Down, int Left, int Right) ParseBandHeaders(this ServerCallContext context)
    {
        return (ParseHeader("x-up"), ParseHeader("x-down"), ParseHeader("x-left"), ParseHeader("x-right"));
        int ParseHeader(string key) => int.TryParse(context.RequestHeaders.GetValue(key), out var value) ? value : 0;
    }

    /// <summary>
    ///     Parses the custom 'x-mode' header to determine the requested game mode.
    /// </summary>
    /// <param name="context">The gRPC server call context.</param>
    /// <returns>The parsed <see cref="GameMode" />, defaulting to <see cref="GameMode.LetterLeagueClassic" /> if not specified or invalid.</returns>
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