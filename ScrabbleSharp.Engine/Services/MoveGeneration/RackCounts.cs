using System.Runtime.CompilerServices;
using ScrabbleSharp.Engine.Core.Rules.Scoring;

namespace ScrabbleSharp.Engine.Services.MoveGeneration;

/// <summary>
///     A helper class to efficiently track the count of each letter tile on a player's rack.
/// </summary>
public sealed class RackCounts
{
    private const int AlphabetSize = 26; // A-Z

    private static readonly char[] LettersByValueDesc;
    private readonly int[] _counts = new int[AlphabetSize + 1]; // +1 for the blank tile '*'

    /// <summary>
    ///     Initializes static members of the <see cref="RackCounts" /> class.
    ///     Creates a sorted list of letters by their Scrabble score to optimize move generation.
    /// </summary>
    static RackCounts()
    {
        var letterList = new List<(char Letter, int Value)>(AlphabetSize);
        for (var character = 'A'; character <= 'Z'; character++)
            letterList.Add((character, ScrabbleTileScores.Table[character]));

        letterList.Sort((a, b) => b.Value.CompareTo(a.Value));
        LettersByValueDesc = letterList.Select(tuple => tuple.Letter).ToArray();
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="RackCounts" /> class from a string of rack letters.
    /// </summary>
    /// <param name="rack">The string of letters on the rack (e.g., "AEIOU**"). Blanks can be represented by '*' or '_'.</param>
    public RackCounts(string rack)
    {
        foreach (var rawChar in rack.ToUpperInvariant())
        {
            var character = rawChar == '_' ? '*' : rawChar;
            _counts[GetIndex(character)]++;
            TilesRemaining++;
        }
    }

    /// <summary>
    ///     Gets the number of tiles remaining on the rack.
    /// </summary>
    public int TilesRemaining { get; private set; }

    /// <summary>
    ///     Gets the array index for a given character ('*' is at the end).
    /// </summary>
    private static int GetIndex(char character) => character == '*' ? AlphabetSize : character - 'A';

    /// <summary>
    ///     Checks if the rack contains a specific tile.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Has(char character) => _counts[GetIndex(character)] > 0;

    /// <summary>
    ///     Removes one instance of a tile from the rack counts.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Take(char character)
    {
        var index = GetIndex(character);
        if (_counts[index] == 0) throw new InvalidOperationException($"Cannot take tile '{character}' not on rack.");
        _counts[index]--;
        TilesRemaining--;
    }

    /// <summary>
    ///     Adds one instance of a tile back to the rack counts.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Put(char character)
    {
        _counts[GetIndex(character)]++;
        TilesRemaining++;
    }

    /// <summary>
    ///     Returns an enumerable of the distinct tiles on the rack, ordered for move generation efficiency.
    ///     Higher-value tiles are returned first, and blanks are returned last.
    /// </summary>
    public IEnumerable<char> DistinctTiles()
    {
        foreach (var character in LettersByValueDesc)
            if (Has(character))
                yield return character;

        if (Has('*')) yield return '*'; // Blank is tried last as it involves the most branching.
    }
}