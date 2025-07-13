using System.Reflection;
using System.Text.RegularExpressions;

namespace ScrabbleSharp.Engine.Core.Dictionary;

/// <summary>
///     Represents a dictionary of words using a trie data structure for efficient lookups.
/// </summary>
public sealed class DictionaryTrie
{
    private static readonly Dictionary<WordList, string> FileNames = new()
    {
        [WordList.Sowpods] = "SOWPODS.txt",
        [WordList.Twl] = "TWL06.txt",
        [WordList.Collins19] = "COLLINS_2019.txt",
        [WordList.Wordnik] = "WORDNIK_21.txt"
    };

    /// <summary>
    ///     Gets the root node of the trie.
    /// </summary>
    public Node Root { get; } = new();

    /// <summary>
    ///     Retrieves the definition for a given word.
    /// </summary>
    /// <param name="word">The word to look up.</param>
    /// <returns>The definition string if found; otherwise, null.</returns>
    public string? GetDefinition(string word)
    {
        return Find(word)?.Definition;
    }

    /// <summary>
    ///     Creates a DictionaryTrie from a word list file at the specified path.
    /// </summary>
    /// <param name="path">The full path to the word list file.</param>
    /// <param name="maxLen">The maximum length of words to include.</param>
    /// <returns>A new <see cref="DictionaryTrie" /> instance.</returns>
    /// <exception cref="FileNotFoundException">Thrown if the file does not exist.</exception>
    public static DictionaryTrie FromFile(string path, int maxLen = int.MaxValue)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException("Word-list file not found.", path);

        using var streamReader = File.OpenText(path);
        return ReadStream(streamReader, maxLen);
    }

    /// <summary>
    ///     Creates a DictionaryTrie from an embedded resource word list.
    /// </summary>
    /// <param name="list">The <see cref="WordList" /> to load.</param>
    /// <param name="maxLen">The maximum length of words to include.</param>
    /// <returns>A new <see cref="DictionaryTrie" /> instance.</returns>
    /// <exception cref="FileNotFoundException">Thrown if the embedded resource is not found.</exception>
    public static DictionaryTrie FromEmbedded(WordList list = WordList.Sowpods,
        int maxLen = int.MaxValue)
    {
        var fileName = FileNames[list];
        var assembly = Assembly.GetExecutingAssembly();

        var resourceId = assembly.GetManifestResourceNames()
            .FirstOrDefault(name => name.EndsWith(fileName,
                StringComparison.OrdinalIgnoreCase));

        if (resourceId is null)
            throw new FileNotFoundException(
                $"Embedded resource '{fileName}' not found. " +
                "Check <EmbeddedResource> wildcard in the .csproj.");

        using var stream = assembly.GetManifestResourceStream(resourceId)!;
        using var streamReader = new StreamReader(stream);
        return ReadStream(streamReader, maxLen);
    }

    /// <summary>
    ///     Loads a collection of words into the trie.
    /// </summary>
    /// <param name="words">The collection of words to add.</param>
    public void LoadWords(IEnumerable<string> words)
    {
        foreach (var word in words) Add(word, null);
    }

    /// <summary>
    ///     Reads words from a TextReader and populates a new trie.
    /// </summary>
    private static DictionaryTrie ReadStream(TextReader streamReader, int maxWordLength)
    {
        var trie = new DictionaryTrie();
        var regex = new Regex("^[A-Z]{2,15}$", RegexOptions.Compiled);

        while (streamReader.ReadLine() is { } line)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            var parts = line.Split('\t', 2); // WORD [TAB definition]
            var word = parts[0].Trim().ToUpperInvariant();
            if (!regex.IsMatch(word) || word.Length > maxWordLength) continue;

            var definition = parts.Length > 1 ? parts[1].Trim() : null;
            trie.Add(word, definition);
        }

        return trie;
    }

    /// <summary>
    ///     Checks if a sequence of characters forms a valid word prefix.
    /// </summary>
    /// <param name="word">The word prefix to check.</param>
    /// <returns>True if the prefix exists in the trie; otherwise, false.</returns>
    public bool Contains(ReadOnlySpan<char> word)
    {
        var currentNode = Root;
        foreach (var character in word)
            if (!currentNode.Children.TryGetValue(character, out currentNode))
                return false;
        return currentNode.IsWord;
    }

    /// <summary>
    ///     Checks if a string is a complete, valid word in the dictionary.
    /// </summary>
    /// <param name="word">The word to check.</param>
    /// <returns>True if the exact word exists; otherwise, false.</returns>
    public bool IsWordExact(string word)
    {
        if (string.IsNullOrWhiteSpace(word))
            return false;

        var node = Find(word.ToUpperInvariant());
        return node is { IsWord: true };
    }

    /// <summary>
    ///     Finds the node corresponding to the end of a given string fragment.
    /// </summary>
    private Node? Find(string fragment)
    {
        var currentNode = Root;
        foreach (var character in fragment.ToUpperInvariant())
            if (!currentNode.Children.TryGetValue(character, out currentNode))
                return null;
        return currentNode;
    }

    /// <summary>
    ///     Adds a word and its optional definition to the trie.
    /// </summary>
    private void Add(string word, string? definition)
    {
        var currentNode = Root;
        foreach (var character in word)
            currentNode = currentNode.Children.TryGetValue(character, out var nextNode)
                ? nextNode
                : currentNode.Children[character] = new Node();

        currentNode.IsWord = true;
        currentNode.Definition ??= definition; // Keep first definition if duplicates
    }

    /// <summary>
    ///     Represents a node in the <see cref="DictionaryTrie" />.
    /// </summary>
    public sealed class Node
    {
        /// <summary>
        ///     Child nodes, keyed by character.
        /// </summary>
        public readonly Dictionary<char, Node> Children = new();

        /// <summary>
        ///     The definition of the word ending at this node.
        /// </summary>
        public string? Definition;

        /// <summary>
        ///     Indicates whether this node represents the end of a valid word.
        /// </summary>
        public bool IsWord;
    }
}