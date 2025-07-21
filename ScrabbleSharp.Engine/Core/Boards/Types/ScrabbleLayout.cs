using ScrabbleSharp.Engine.Core.Boards.Interfaces;
using ScrabbleSharp.Engine.Core.Tiles;
using static ScrabbleSharp.Engine.Core.Tiles.MultiplierType;

namespace ScrabbleSharp.Engine.Core.Boards.Types;

/// <summary>
///     Represents the standard 15x15 Scrabble board layout.
/// </summary>
public sealed class ScrabbleLayout : IBoardLayout
{
    private static readonly MultiplierType[,] Kernel =
    {
        {
            TripleWord, None, None, DoubleLetter, None, None, None, TripleWord, None, None, None, DoubleLetter, None,
            None, TripleWord
        },
        {
            None, DoubleWord, None, None, None, TripleLetter, None, None, None, TripleLetter, None, None, None,
            DoubleWord, None
        },
        {
            None, None, DoubleWord, None, None, None, DoubleLetter, None, DoubleLetter, None, None, None, DoubleWord,
            None, None
        },
        {
            DoubleLetter, None, None, DoubleWord, None, None, None, DoubleLetter, None, None, None, DoubleWord, None,
            None, DoubleLetter
        },
        { None, None, None, None, DoubleWord, None, None, None, None, None, DoubleWord, None, None, None, None },
        {
            None, TripleLetter, None, None, None, TripleLetter, None, None, None, TripleLetter, None, None, None,
            TripleLetter, None
        },
        {
            None, None, DoubleLetter, None, None, None, DoubleLetter, None, DoubleLetter, None, None, None,
            DoubleLetter, None, None
        },
        {
            TripleWord, None, None, DoubleLetter, None, None, None, DoubleWord, None, None, None, DoubleLetter, None,
            None, TripleWord
        },
        {
            None, None, DoubleLetter, None, None, None, DoubleLetter, None, DoubleLetter, None, None, None,
            DoubleLetter, None, None
        },
        {
            None, TripleLetter, None, None, None, TripleLetter, None, None, None, TripleLetter, None, None, None,
            TripleLetter, None
        },
        { None, None, None, None, DoubleWord, None, None, None, None, None, DoubleWord, None, None, None, None },
        {
            DoubleLetter, None, None, DoubleWord, None, None, None, DoubleLetter, None, None, None, DoubleWord, None,
            None, DoubleLetter
        },
        {
            None, None, DoubleWord, None, None, None, DoubleLetter, None, DoubleLetter, None, None, None, DoubleWord,
            None, None
        },
        {
            None, DoubleWord, None, None, None, TripleLetter, None, None, None, TripleLetter, None, None, None,
            DoubleWord, None
        },
        {
            TripleWord, None, None, DoubleLetter, None, None, None, TripleWord, None, None, None, DoubleLetter, None,
            None, TripleWord
        }
    };

    /// <inheritdoc />
    public int Rows => 15;

    /// <inheritdoc />
    public int Cols => 15;

    /// <inheritdoc />
    public int OriginRow => 7;

    /// <inheritdoc />
    public int OriginCol => 7;

    /// <inheritdoc />
    public MultiplierType GetMultiplier(int row, int column)
    {
        if (row is < 0 or >= 15 || column is < 0 or >= 15)
            throw new ArgumentOutOfRangeException($"({row},{column}) outside 15×15 kernel.");

        return Kernel[row, column];
    }
}