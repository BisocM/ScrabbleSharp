using ScrabbleSharp.Engine.Core.Boards.Interfaces;
using ScrabbleSharp.Engine.Core.Tiles;
using static ScrabbleSharp.Engine.Core.Tiles.MultiplierType;

namespace ScrabbleSharp.Engine.Core.Boards.Types;

/// <summary>
///     Represents the 21x21 board layout for Super Scrabble.
/// </summary>
public sealed class ScrabbleSuperLayout : IBoardLayout
{
    /// <summary>
    ///     The size (width and height) of the Super Scrabble board.
    /// </summary>
    public const int Size = 21;

    private static readonly MultiplierType[,] Kernel = new MultiplierType[Size, Size]
    {
        {
            QuadrupleWord, None, None, DoubleLetter, None, None, None, TripleWord, None, None, DoubleLetter, None, None,
            TripleWord, None, None, None, DoubleLetter, None, None, QuadrupleWord
        },
        {
            None, DoubleWord, None, None, TripleLetter, None, None, None, DoubleWord, None, None, None, DoubleWord,
            None, None, None, TripleLetter, None, None, DoubleWord, None
        },
        {
            None, None, DoubleWord, None, None, QuadrupleLetter, None, None, None, DoubleWord, None, DoubleWord, None,
            None, None, QuadrupleLetter, None, None, DoubleWord, None, None
        },
        {
            DoubleLetter, None, None, TripleWord, None, None, DoubleLetter, None, None, None, TripleWord, None, None,
            None, DoubleLetter, None, None, TripleWord, None, None, DoubleLetter
        },
        {
            None, TripleLetter, None, None, DoubleWord, None, None, None, TripleLetter, None, None, None, TripleLetter,
            None, None, None, DoubleWord, None, None, TripleLetter, None
        },
        {
            None, None, QuadrupleLetter, None, None, DoubleWord, None, None, None, DoubleLetter, None, DoubleLetter,
            None, None, None, DoubleWord, None, None, QuadrupleLetter, None, None
        },
        {
            None, None, None, DoubleLetter, None, None, DoubleWord, None, None, None, DoubleLetter, None, None, None,
            DoubleWord, None, None, DoubleLetter, None, None, None
        },
        {
            TripleWord, None, None, None, None, None, None, DoubleWord, None, None, None, None, None, DoubleWord, None,
            None, None, None, None, None, TripleWord
        },
        {
            None, DoubleWord, None, None, TripleLetter, None, None, None, TripleLetter, None, None, None, TripleLetter,
            None, None, None, TripleLetter, None, None, DoubleWord, None
        },
        {
            None, None, DoubleWord, None, None, DoubleLetter, None, None, None, DoubleLetter, None, DoubleLetter, None,
            None, None, DoubleLetter, None, None, DoubleWord, None, None
        },
        {
            DoubleLetter, None, None, TripleWord, None, None, DoubleLetter, None, None, None, None, None, None, None,
            DoubleLetter, None, None, TripleWord, None, None, DoubleLetter
        },
        {
            None, None, DoubleWord, None, None, DoubleLetter, None, None, None, DoubleLetter, None, DoubleLetter, None,
            None, None, DoubleLetter, None, None, DoubleWord, None, None
        },
        {
            None, DoubleWord, None, None, TripleLetter, None, None, None, TripleLetter, None, None, None, TripleLetter,
            None, None, None, TripleLetter, None, None, DoubleWord, None
        },
        {
            TripleWord, None, None, None, None, None, None, DoubleWord, None, None, None, None, None, DoubleWord, None,
            None, None, None, None, None, TripleWord
        },
        {
            None, None, None, DoubleLetter, None, None, DoubleWord, None, None, None, DoubleLetter, None, None, None,
            DoubleWord, None, None, DoubleLetter, None, None, None
        },
        {
            None, None, QuadrupleLetter, None, None, DoubleWord, None, None, None, DoubleLetter, None, DoubleLetter,
            None, None, None, DoubleWord, None, None, QuadrupleLetter, None, None
        },
        {
            None, TripleLetter, None, None, DoubleWord, None, None, None, TripleLetter, None, None, None, TripleLetter,
            None, None, None, DoubleWord, None, None, TripleLetter, None
        },
        {
            DoubleLetter, None, None, TripleWord, None, None, DoubleLetter, None, None, None, TripleWord, None, None,
            None, DoubleLetter, None, None, TripleWord, None, None, DoubleLetter
        },
        {
            None, None, DoubleWord, None, None, QuadrupleLetter, None, None, None, DoubleWord, None, DoubleWord, None,
            None, None, QuadrupleLetter, None, None, DoubleWord, None, None
        },
        {
            None, DoubleWord, None, None, TripleLetter, None, None, None, DoubleWord, None, None, None, DoubleWord,
            None, None, None, TripleLetter, None, None, DoubleWord, None
        },
        {
            QuadrupleWord, None, None, DoubleLetter, None, None, None, TripleWord, None, None, DoubleLetter, None, None,
            TripleWord, None, None, None, DoubleLetter, None, None, QuadrupleWord
        }
    };

    /// <inheritdoc />
    public int Rows => Size;

    /// <inheritdoc />
    public int Cols => Size;

    /// <inheritdoc />
    public int OriginRow => 10;

    /// <inheritdoc />
    public int OriginCol => 10;

    /// <inheritdoc />
    public MultiplierType GetMultiplier(int row, int column)
    {
        if (row is < 0 or >= Size || column is < 0 or >= Size)
            throw new ArgumentOutOfRangeException($"({row},{column}) is outside the {Size} × {Size} board.");

        return Kernel[row, column];
    }
}