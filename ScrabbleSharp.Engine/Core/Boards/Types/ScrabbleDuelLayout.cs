using ScrabbleSharp.Engine.Core.Boards.Interfaces;
using ScrabbleSharp.Engine.Core.Tiles;
using static ScrabbleSharp.Engine.Core.Tiles.MultiplierType;

namespace ScrabbleSharp.Engine.Core.Boards.Types;

/// <summary>
///     Represents the fixed 11x11 board layout for Scrabble Duel.
/// </summary>
public sealed class ScrabbleDuelLayout : IBoardLayout
{
    /// <summary>
    ///     The size (width and height) of the Scrabble Duel board.
    /// </summary>
    public const int Size = 11;

    private static readonly MultiplierType[,] Kernel = new MultiplierType[Size, Size]
    {
        { TripleWord, None, None, None, DoubleLetter, None, DoubleLetter, None, None, None, TripleWord },
        { None, DoubleWord, None, TripleLetter, None, None, None, TripleLetter, None, DoubleWord, None },
        { None, None, DoubleWord, None, None, DoubleLetter, None, None, DoubleWord, None, None },
        { None, TripleLetter, None, TripleLetter, None, None, None, TripleLetter, None, TripleLetter, None },
        { DoubleLetter, None, None, None, DoubleWord, None, DoubleWord, None, None, None, DoubleLetter },
        { None, None, DoubleLetter, None, None, DoubleWord, None, None, DoubleLetter, None, None },
        { DoubleLetter, None, None, None, DoubleWord, None, DoubleWord, None, None, None, DoubleLetter },
        { None, TripleLetter, None, TripleLetter, None, None, None, TripleLetter, None, TripleLetter, None },
        { None, None, DoubleWord, None, None, DoubleLetter, None, None, DoubleWord, None, None },
        { None, DoubleWord, None, TripleLetter, None, None, None, TripleLetter, None, DoubleWord, None },
        { TripleWord, None, None, None, DoubleLetter, None, DoubleLetter, None, None, None, TripleWord }
    };

    /// <inheritdoc />
    public int Rows => Size;

    /// <inheritdoc />
    public int Cols => Size;

    /// <inheritdoc />
    public int OriginRow => 5;

    /// <inheritdoc />
    public int OriginCol => 5;

    /// <inheritdoc />
    public MultiplierType GetMultiplier(int row, int column)
    {
        if (row < 0 || row >= Size || column < 0 || column >= Size)
            throw new ArgumentOutOfRangeException(nameof(row),
                $"({row},{column}) is outside the {Size} × {Size} board.");

        return Kernel[row, column];
    }
}