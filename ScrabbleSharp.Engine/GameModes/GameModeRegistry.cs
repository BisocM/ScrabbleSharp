using System.Collections.Concurrent;
using ScrabbleSharp.Engine.Core.Dictionary;
using ScrabbleSharp.Engine.Core.Rules.Interfaces;
using ScrabbleSharp.Engine.GameModes.Enums;

namespace ScrabbleSharp.Engine.GameModes;

/// <summary>
///     A registry for accessing game mode configurations and their associated resources, like dictionaries.
/// </summary>
public sealed class GameModeRegistry(IEnumerable<IGameMode> providers)
{
    // Caches dictionary tries to avoid reloading them from disk/resources for each request.
    private static readonly ConcurrentDictionary<WordList, DictionaryTrie> TrieCache = new();

    /// <summary>
    ///     A read-only dictionary of all registered game modes, keyed by their <see cref="GameMode" /> ID.
    /// </summary>
    public readonly IReadOnlyDictionary<GameMode, IGameMode> Modes = providers.ToDictionary(provider => provider.Id);

    /// <summary>
    ///     Retrieves the <see cref="IGameMode" /> provider for a specified game mode ID.
    /// </summary>
    /// <param name="mode">The ID of the game mode to retrieve.</param>
    /// <returns>The corresponding <see cref="IGameMode" /> provider.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the game mode is not supported.</exception>
    public IGameMode Get(GameMode mode) =>
        Modes.TryGetValue(mode, out var gameMode)
            ? gameMode
            : throw new ArgumentOutOfRangeException(nameof(mode), mode, "Unsupported game mode.");

    /// <summary>
    ///     Retrieves the <see cref="DictionaryTrie" /> for a specified word list, using a cache to ensure efficiency.
    /// </summary>
    /// <param name="list">The <see cref="WordList" /> for which to get the trie.</param>
    /// <returns>A cached or newly created <see cref="DictionaryTrie" /> for the specified list.</returns>
    public DictionaryTrie GetTrie(WordList list) =>
        TrieCache.GetOrAdd(list, wordList => DictionaryTrie.FromEmbedded(wordList));
}