using Microsoft.Extensions.DependencyInjection;
using ScrabbleSharp.Engine.Core.Boards.Interfaces;
using ScrabbleSharp.Engine.Core.Boards.Types;
using ScrabbleSharp.Engine.Core.Dictionary;
using ScrabbleSharp.Engine.Core.Rules.Interfaces;
using ScrabbleSharp.Engine.Core.Rules.Types;
using ScrabbleSharp.Engine.GameModes.Enums;

namespace ScrabbleSharp.Engine.GameModes.Types;

/// <summary>
///     Defines the configuration for the standard Scrabble Classic game mode.
/// </summary>
public sealed class ScrabbleClassicMode : IGameMode
{
    /// <inheritdoc />
    public GameMode Id => GameMode.ScrabbleClassic;

    /// <inheritdoc />
    public IGameRules Rules { get; } = new ScrabbleClassicRules();

    /// <inheritdoc />
    public WordList Dictionary => WordList.Collins19;

    /// <inheritdoc />
    public IBoardLayout CreateLayout(IServiceProvider services)
        => services.GetRequiredService<ScrabbleLayout>();
}