using ScrabbleSharp.Engine.Core.Boards.Interfaces;
using ScrabbleSharp.Engine.Core.Dictionary;
using ScrabbleSharp.Engine.GameModes.Enums;

namespace ScrabbleSharp.Engine.Core.Rules.Interfaces;

/// <summary>
///     Defines a complete game mode, encapsulating the rules, board layout, and dictionary.
/// </summary>
public interface IGameMode
{
    /// <summary>
    ///     Gets the unique identifier for the game mode.
    /// </summary>
    GameMode Id { get; }

    /// <summary>
    ///     Gets the rules engine for this game mode.
    /// </summary>
    IGameRules Rules { get; }

    /// <summary>
    ///     Gets the dictionary used for this game mode.
    /// </summary>
    WordList Dictionary { get; }

    /// <summary>
    ///     Creates a new board layout specific to this game mode.
    /// </summary>
    /// <param name="services">A service provider for dependency injection.</param>
    /// <returns>A new instance of <see cref="IBoardLayout" />.</returns>
    IBoardLayout CreateLayout(IServiceProvider services);
}