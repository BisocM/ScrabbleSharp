using ScrabbleSharp.Engine.Core.Boards.Interfaces;
using ScrabbleSharp.Engine.Core.Dictionary;
using ScrabbleSharp.Engine.GameModes.Enums;

namespace ScrabbleSharp.Engine.Core.Rules.Interfaces;

/// <summary>
///     Defines a complete game mode, encapsulating the rules, dictionary, and board layout for a specific variant of Scrabble.
/// </summary>
public interface IGameMode
{
    /// <summary>
    ///     Gets the unique identifier for this game mode.
    /// </summary>
    GameMode Id { get; }

    /// <summary>
    ///     Gets the game rules associated with this mode.
    /// </summary>
    IGameRules Rules { get; }

    /// <summary>
    ///     Gets the word list (dictionary) used for this game mode.
    /// </summary>
    WordList Dictionary { get; }

    /// <summary>
    ///     Creates a new instance of the board layout for this game mode.
    /// </summary>
    /// <param name="services">A service provider to resolve layout dependencies.</param>
    /// <returns>A new <see cref="IBoardLayout" /> instance.</returns>
    IBoardLayout CreateLayout(IServiceProvider services);
}