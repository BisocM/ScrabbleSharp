using System.Collections.Concurrent;
using ScrabbleSharp.Engine.Core.Dictionary;
using ScrabbleSharp.Engine.Core.Rules.Interfaces;
using ScrabbleSharp.Engine.GameModes.Enums;

namespace ScrabbleSharp.Engine.GameModes;

/// <summary>
///     A registry for all available game modes and a cache for their associated dictionaries.
/// </summary>
public sealed class GameModeRegistry(IEnumerable<IGameMode> providers)
{
    private static readonly ConcurrentDictionary<WordList, DictionaryTrie> TrieCache = new();

    /// <summary>
    ///     Gets a read-only dictionary of all registered game modes, keyed by their <see cref="GameMode" /> enum.
    /// </summary>
    public readonly IReadOnlyDictionary<GameMode, IGameMode> Modes = providers.ToDictionary(provider => provider.Id);

    /// <summary>
    ///     Retrieves the game mode provider for the specified game mode.
    /// </summary>
    /// <param name="mode">The <see cref="GameMode" /> identifier.</param>
    /// <returns>The corresponding <see cref="IGameMode" /> provider.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the game mode is not supported.</exception>
    public IGameMode Get(GameMode mode) =>
        Modes.TryGetValue(mode, out var gameMode)
            ? gameMode
            : throw new ArgumentOutOfRangeException(nameof(mode), mode, "Unsupported game mode.");

    /// <summary>
    ///     Retrieves the dictionary trie for a given word list, using a cache to avoid reloading.
    /// </summary>
    /// <param name="list">The <see cref="WordList" /> to load.</param>
    /// <returns>A <see cref="DictionaryTrie" /> for the specified word list.</returns>
    public DictionaryTrie GetTrie(WordList list) =>
        TrieCache.GetOrAdd(list, wordList => DictionaryTrie.FromEmbedded(wordList));
}