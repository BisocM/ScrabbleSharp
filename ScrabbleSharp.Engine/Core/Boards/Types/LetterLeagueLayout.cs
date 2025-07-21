using ScrabbleSharp.Contracts.Protos;
using ScrabbleSharp.Engine.Core.Boards.Interfaces;
using ScrabbleSharp.Engine.Core.Tiles;
using ScrabbleSharp.Engine.Core.Utils;
using static ScrabbleSharp.Engine.Core.Tiles.MultiplierType;

namespace ScrabbleSharp.Engine.Core.Boards.Types;

/// <summary>
///     Implements the expandable board layout for Letter League.
/// </summary>
/// <remarks>
///     This layout starts with a 15x15 kernel and can be expanded by adding 4-cell wide "bands"
///     in any of the four cardinal directions. The multiplier pattern repeats periodically.
/// </remarks>
public sealed class LetterLeagueLayout : IExpandableBoardLayout
{
    /// <summary>
    ///     The size of the initial, non-expanded board kernel.
    /// </summary>
    private const int KernelSize = 15;

    /// <summary>
    ///     The width/height of an expansion band in cells.
    /// </summary>
    private const int Band = 4;

    /// <summary>
    ///     The starting index for the periodic folding of multiplier coordinates.
    /// </summary>
    private const int RepeatStart = 3;

    /// <summary>
    ///     The period over which the multiplier pattern repeats.
    /// </summary>
    private const int RepeatPeriod = 12;

    /// <summary>
    ///     The default maximum number of expansion bands allowed in any single direction.
    /// </summary>
    private const int DefaultMaxBandsPerDirection = 4;


    /// <summary>
    ///     The 15x15 multiplier kernel for the central part of the board.
    /// </summary>
    private static readonly MultiplierType[,] Kernel =
    {
        { TripleLetter, None, TripleLetter, None, DoubleLetter, None, None, None, None, None, DoubleLetter, None, TripleLetter, None, TripleLetter },
        { None, TripleWord, None, None, None, None, DoubleLetter, DoubleWord, DoubleLetter, None, None, None, None, TripleWord, None },
        { TripleLetter, None, TripleLetter, None, DoubleLetter, None, None, None, None, None, DoubleLetter, None, TripleLetter, None, TripleLetter },
        { None, None, None, DoubleWord, None, None, None, DoubleLetter, None, None, None, DoubleWord, None, None, None },
        { DoubleLetter, None, DoubleLetter, None, None, DoubleLetter, None, None, None, DoubleLetter, None, None, DoubleLetter, None, DoubleLetter },
        { None, None, None, None, DoubleWord, None, None, None, None, None, DoubleWord, None, None, None, None },
        { None, DoubleLetter, None, None, None, None, TripleLetter, None, TripleLetter, None, None, None, None, DoubleLetter, None },
        { None, DoubleWord, None, None, None, DoubleWord, None, None, None, DoubleWord, None, None, None, DoubleWord, None },
        { None, DoubleLetter, None, None, None, None, TripleLetter, None, TripleLetter, None, None, None, None, DoubleLetter, None },
        { None, None, None, None, DoubleWord, None, None, None, None, None, DoubleWord, None, None, None, None },
        { DoubleLetter, None, DoubleLetter, None, None, DoubleLetter, None, None, None, DoubleLetter, None, None, DoubleLetter, None, DoubleLetter },
        { None, None, None, DoubleWord, None, None, None, DoubleLetter, None, None, None, DoubleWord, None, None, None },
        { TripleLetter, None, TripleLetter, None, DoubleLetter, None, None, None, None, None, DoubleLetter, None, TripleLetter, None, TripleLetter },
        { None, TripleWord, None, None, None, None, DoubleLetter, DoubleWord, DoubleLetter, None, None, None, None, TripleWord, None },
        { TripleLetter, None, TripleLetter, None, DoubleLetter, None, None, None, None, None, DoubleLetter, None, TripleLetter, None, TripleLetter }
    };

    private int _maxDownBands = DefaultMaxBandsPerDirection;
    private int _maxLeftBands = DefaultMaxBandsPerDirection;
    private int _maxRightBands = DefaultMaxBandsPerDirection;

    private int _maxUpBands = DefaultMaxBandsPerDirection;

    private int _shiftCol;
    private int _shiftRow;

    private int _upBands, _downBands, _leftBands, _rightBands;

    /// <summary>
    ///     Initializes a new instance of the <see cref="LetterLeagueLayout" /> class with optional expansion limits.
    /// </summary>
    public LetterLeagueLayout(
        int? maxUpBands = null,
        int? maxDownBands = null,
        int? maxLeftBands = null,
        int? maxRightBands = null)
    {
        SetExpansionLimits(maxUpBands, maxDownBands, maxLeftBands, maxRightBands);
    }

    /// <inheritdoc />
    public int Rows { get; private set; } = KernelSize;

    /// <inheritdoc />
    public int Cols { get; private set; } = KernelSize;

    /// <inheritdoc />
    public int OriginRow => _shiftRow + 7;

    /// <inheritdoc />
    public int OriginCol => _shiftCol + 7;

    /// <inheritdoc />
    public void Reset()
    {
        Rows = Cols = KernelSize;
        _shiftRow = _shiftCol = 0;
        _upBands = _downBands = _leftBands = _rightBands = 0;
    }

    /// <inheritdoc />
    public void SetExpansionLimits(
        int? up = null,
        int? down = null,
        int? left = null,
        int? right = null)
    {
        if (up.HasValue) _maxUpBands = Math.Clamp(up.Value, 0, DefaultMaxBandsPerDirection);
        if (down.HasValue) _maxDownBands = Math.Clamp(down.Value, 0, DefaultMaxBandsPerDirection);
        if (left.HasValue) _maxLeftBands = Math.Clamp(left.Value, 0, DefaultMaxBandsPerDirection);
        if (right.HasValue) _maxRightBands = Math.Clamp(right.Value, 0, DefaultMaxBandsPerDirection);
    }

    /// <inheritdoc />
    public ExpandDelta? TryExpandAt(int row, int column)
    {
        Direction? dir = null;

        // Determine if the specified coordinate is within an expansion trigger zone.
        if (row < Band) dir = Direction.Up;
        else if (Rows - 1 - row < Band) dir = Direction.Down;
        else if (column < Band) dir = Direction.Left;
        else if (Cols - 1 - column < Band) dir = Direction.Right;

        if (dir is null) return null;

        // Check if the expansion limit for the determined direction has been reached.
        return dir switch
        {
            Direction.Up when _upBands >= _maxUpBands => null,
            Direction.Down when _downBands >= _maxDownBands => null,
            Direction.Left when _leftBands >= _maxLeftBands => null,
            Direction.Right when _rightBands >= _maxRightBands => null,
            _ => PerformExpansion(dir.Value)
        };
    }

    /// <inheritdoc />
    public MultiplierType GetMultiplier(int row, int column)
    {
        if (row < 0 || column < 0 || row >= Rows || column >= Cols)
            throw new ArgumentOutOfRangeException(
                $"({row},{column}) is outside the current {Rows} × {Cols} board.");
        return MapMultiplier(row, column, _shiftRow, _shiftCol);
    }


    /// <summary>
    ///     Executes the board expansion in a given direction.
    /// </summary>
    /// <param name="direction">The direction to expand in.</param>
    /// <returns>An <see cref="ExpandDelta" /> describing the changes to the board.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown for an invalid direction.</exception>
    private ExpandDelta PerformExpansion(Direction direction)
    {
        int oldRows = Rows, oldCols = Cols;
        var newRows = Rows + (direction is Direction.Up or Direction.Down ? Band : 0);
        var newCols = Cols + (direction is Direction.Left or Direction.Right ? Band : 0);
        var newShiftRow = _shiftRow + (direction == Direction.Up ? Band : 0);
        var newShiftCol = _shiftCol + (direction == Direction.Left ? Band : 0);

        Rows = newRows;
        Cols = newCols;
        _shiftRow = newShiftRow;
        _shiftCol = newShiftCol;

        switch (direction)
        {
            case Direction.Up: _upBands++; break;
            case Direction.Down: _downBands++; break;
            case Direction.Left: _leftBands++; break;
            case Direction.Right: _rightBands++; break;
            default: throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
        }


        int offsetRow, offsetCol, sliceRows, sliceCols;
        switch (direction)
        {
            case Direction.Up:
                offsetRow = 0;
                offsetCol = 0;
                sliceRows = Band;
                sliceCols = newCols;
                break;
            case Direction.Down:
                offsetRow = oldRows;
                offsetCol = 0;
                sliceRows = Band;
                sliceCols = newCols;
                break;
            case Direction.Left:
                offsetRow = 0;
                offsetCol = 0;
                sliceRows = newRows;
                sliceCols = Band;
                break;
            default: // Right
                offsetRow = 0;
                offsetCol = oldCols;
                sliceRows = newRows;
                sliceCols = Band;
                break;
        }

        var delta = new ExpandDelta
        {
            TotalRows = (uint)newRows,
            TotalCols = (uint)newCols,
            ShiftRow = newShiftRow,
            ShiftCol = newShiftCol,
            OffsetRow = (uint)offsetRow,
            OffsetCol = (uint)offsetCol,
            Rows = (uint)sliceRows,
            Cols = (uint)sliceCols,
            Multipliers = { Capacity = sliceRows * sliceCols }
        };

        // Populate the delta with the multipliers for the newly added slice.
        for (var r = 0; r < sliceRows; r++)
        for (var c = 0; c < sliceCols; c++)
        {
            var m = MapMultiplier(offsetRow + r, offsetCol + c, newShiftRow, newShiftCol);
            delta.Multipliers.Add(m.ToProto());
        }

        return delta;
    }

    /// <summary>
    ///     Maps absolute board coordinates to a multiplier type using a periodic folding algorithm.
    /// </summary>
    /// <param name="absoluteRow">The absolute row on the expanded board.</param>
    /// <param name="absoluteCol">The absolute column on the expanded board.</param>
    /// <param name="shiftRow">The current row shift due to upward expansions.</param>
    /// <param name="shiftCol">The current column shift due to leftward expansions.</param>
    /// <returns>The calculated <see cref="MultiplierType" />.</returns>
    private static MultiplierType MapMultiplier(int absoluteRow, int absoluteCol, int shiftRow, int shiftCol)
    {
        var relativeRow = absoluteRow - shiftRow;
        var relativeCol = absoluteCol - shiftCol;

        var kernelRow = Fold(relativeRow);
        var kernelCol = Fold(relativeCol);

        var raw = Kernel[kernelRow, kernelCol];

        // The center square (7,7) in the kernel is a Double Word score.
        // In expanded regions, this position should not be a premium square.
        var isFirstKernel =
            relativeRow is >= 0 and < KernelSize &&
            relativeCol is >= 0 and < KernelSize;

        return !isFirstKernel && kernelRow == 7 && kernelCol == 7
            ? None
            : raw;

        //Folds a coordinate into the repeating pattern of the kernel.
        static int Fold(int x) =>
            x is >= 0 and < RepeatStart
                ? x
                : RepeatStart + (x - RepeatStart).Mod(RepeatPeriod);
    }
}