using System.Runtime.CompilerServices;
using ScrabbleSharp.Engine.Core.Rules.Scoring;

namespace ScrabbleSharp.Engine.Services.MoveGeneration;

/// <summary>
///     A helper class to efficiently track the counts of available tiles on a player's rack.
/// </summary>
public sealed class RackCounts
{
    private const int AlphabetSize = 26; // A-Z

    private static readonly char[] LettersByValueDesc;
    private readonly int[] _counts = new int[AlphabetSize + 1]; // +1 for the blank tile '*'

    /// <summary>
    ///     Initializes the static list of letters sorted by descending point value.
    ///     This is used to prioritize trying higher-scoring letters first during move generation.
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
    ///     Initializes a new instance of the <see cref="RackCounts" /> class from a string representation of a rack.
    /// </summary>
    /// <param name="rack">The string of rack letters (e.g., "AEIOU*_"). Blanks can be represented by '*' or '_'.</param>
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
    ///     Gets the total number of tiles currently remaining on the rack.
    /// </summary>
    public int TilesRemaining { get; private set; }

    /// <summary>
    ///     Gets the array index for a given tile character.
    /// </summary>
    private static int GetIndex(char character) => character == '*' ? AlphabetSize : character - 'A';

    /// <summary>
    ///     Checks if the rack contains at least one of the specified tile.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Has(char character) => _counts[GetIndex(character)] > 0;

    /// <summary>
    ///     Decrements the count of a specified tile, effectively "taking" it from the rack.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the tile is not on the rack.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Take(char character)
    {
        var index = GetIndex(character);
        if (_counts[index] == 0) throw new InvalidOperationException($"Cannot take tile '{character}' not on rack.");
        _counts[index]--;
        TilesRemaining--;
    }

    /// <summary>
    ///     Increments the count of a specified tile, effectively "putting" it back on the rack.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Put(char character)
    {
        _counts[GetIndex(character)]++;
        TilesRemaining++;
    }

    /// <summary>
    ///     Returns an enumerable of the distinct tiles on the rack, ordered for optimal move generation.
    /// </summary>
    /// <remarks>
    ///     High-value letters are returned first, and the blank tile ('*') is returned last
    ///     because it creates the most branching in the search algorithm.
    /// </remarks>
    public IEnumerable<char> DistinctTiles()
    {
        foreach (var character in LettersByValueDesc)
            if (Has(character))
                yield return character;

        if (Has('*')) yield return '*';
    }
}