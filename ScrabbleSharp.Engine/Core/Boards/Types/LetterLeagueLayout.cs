using ScrabbleSharp.Engine.Core.Boards.Interfaces;
using ScrabbleSharp.Engine.Core.Utils;
using ScrabbleSharp.Contracts.Protos;
using ScrabbleSharp.Engine.Core.Tiles;
using static ScrabbleSharp.Engine.Core.Tiles.MultiplierType;

namespace ScrabbleSharp.Engine.Core.Boards.Types
{
    /// <summary>
    /// Implements the dynamic Letter League board:
    /// <para>- Starts as a 15 × 15 Scrabble-sized kernel.</para>
    /// <para>- Can expand in four-row / four-column “bands” up to a hard
    /// limit of four bands per side ( &lt;= 40 × 40 overall ).</para>
    /// <para>- Multiplier pattern repeats every 12 squares beyond the first
    /// three rows/columns of the kernel.</para>
    /// </summary>
    public sealed class LetterLeagueLayout : IExpandableBoardLayout
    {
        /*======================== configuration ============================*/
        private const int KernelSize   = 15;
        private const int Band         = 4;
        private const int RepeatStart  = 3;
        private const int RepeatPeriod = 12;

        /// <summary>Absolute ceiling of expansion bands per direction.</summary>
        private const int DefaultMaxBandsPerDirection = 4;

        /*======================= static kernel template =====================*/
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

        /*====================== instance-level state =======================*/
        private int _rows      = KernelSize;
        private int _cols      = KernelSize;
        private int _shiftRow  = 0;
        private int _shiftCol  = 0;

        private int _upBands, _downBands, _leftBands, _rightBands;

        private int _maxUpBands    = DefaultMaxBandsPerDirection;
        private int _maxDownBands  = DefaultMaxBandsPerDirection;
        private int _maxLeftBands  = DefaultMaxBandsPerDirection;
        private int _maxRightBands = DefaultMaxBandsPerDirection;

        /*========================= Construction =============================*/
        public LetterLeagueLayout(
            int? maxUpBands    = null,
            int? maxDownBands  = null,
            int? maxLeftBands  = null,
            int? maxRightBands = null)
        {
            SetExpansionLimits(maxUpBands, maxDownBands, maxLeftBands, maxRightBands);
        }
        
        public int Rows      => _rows;
        public int Cols      => _cols;
        public int OriginRow => _shiftRow + 7;
        public int OriginCol => _shiftCol + 7;

        /// <summary>Restores the layout to its pristine 15 × 15 state.</summary>
        public void Reset()
        {
            _rows = _cols = KernelSize;
            _shiftRow = _shiftCol = 0;
            _upBands = _downBands = _leftBands = _rightBands = 0;
        }

        /// <summary>
        /// Sets (and clamps) per-direction expansion ceilings.  Values larger
        /// than <see cref="DefaultMaxBandsPerDirection"/> are silently reduced
        /// to that maximum.
        /// </summary>
        public void SetExpansionLimits(
            int? up    = null,
            int? down  = null,
            int? left  = null,
            int? right = null)
        {
            if (up.HasValue)    _maxUpBands    = Math.Clamp(up.Value,    0, DefaultMaxBandsPerDirection);
            if (down.HasValue)  _maxDownBands  = Math.Clamp(down.Value,  0, DefaultMaxBandsPerDirection);
            if (left.HasValue)  _maxLeftBands  = Math.Clamp(left.Value,  0, DefaultMaxBandsPerDirection);
            if (right.HasValue) _maxRightBands = Math.Clamp(right.Value, 0, DefaultMaxBandsPerDirection);
        }

        /// <summary>
        /// Attempts to expand the board if the given square lies inside the
        /// “trigger band”.  The call is refused once the configured limit for
        /// that direction is reached.
        /// </summary>
        public ExpandDelta? TryExpandAt(int row, int column)
        {
            Direction? dir = null;

            if (row < Band)                     dir = Direction.Up;
            else if (_rows - 1 - row < Band)    dir = Direction.Down;
            else if (column < Band)             dir = Direction.Left;
            else if (_cols - 1 - column < Band) dir = Direction.Right;

            if (dir is null) return null;

            return dir switch
            {
                Direction.Up when _upBands >= _maxUpBands => null,
                Direction.Down when _downBands >= _maxDownBands => null,
                Direction.Left when _leftBands >= _maxLeftBands => null,
                Direction.Right when _rightBands >= _maxRightBands => null,
                _ => PerformExpansion(dir.Value)
            };
        }

        public MultiplierType GetMultiplier(int row, int column)
        {
            if (row < 0 || column < 0 || row >= _rows || column >= _cols)
                throw new ArgumentOutOfRangeException(
                    $"({row},{column}) is outside the current {_rows} × {_cols} board.");
            return MapMultiplier(row, column, _shiftRow, _shiftCol);
        }

        /*========================= Private Helpers ==========================*/

        /// <summary>Actually grows the board by one <see cref="Band"/>.</summary>
        private ExpandDelta PerformExpansion(Direction direction)
        {
            int oldRows = _rows, oldCols = _cols;
            int newRows = _rows + (direction is Direction.Up or Direction.Down   ? Band : 0);
            int newCols = _cols + (direction is Direction.Left or Direction.Right ? Band : 0);
            int newShiftRow = _shiftRow + (direction == Direction.Up   ? Band : 0);
            int newShiftCol = _shiftCol + (direction == Direction.Left ? Band : 0);

            _rows      = newRows;
            _cols      = newCols;
            _shiftRow  = newShiftRow;
            _shiftCol  = newShiftCol;

            switch (direction)
            {
                case Direction.Up    : _upBands++;    break;
                case Direction.Down  : _downBands++;  break;
                case Direction.Left  : _leftBands++;  break;
                case Direction.Right : _rightBands++; break;
                default : throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }

            /* build delta restricted to the new slice */
            int offsetRow, offsetCol, sliceRows, sliceCols;
            switch (direction)
            {
                case Direction.Up:
                    offsetRow  = 0;        offsetCol  = 0;
                    sliceRows  = Band;     sliceCols  = newCols; break;
                case Direction.Down:
                    offsetRow  = oldRows;  offsetCol  = 0;
                    sliceRows  = Band;     sliceCols  = newCols; break;
                case Direction.Left:
                    offsetRow  = 0;        offsetCol  = 0;
                    sliceRows  = newRows;  sliceCols  = Band;    break;
                default: // Right
                    offsetRow  = 0;        offsetCol  = oldCols;
                    sliceRows  = newRows;  sliceCols  = Band;    break;
            }

            var delta = new ExpandDelta
            {
                TotalRows  = (uint)newRows,
                TotalCols  = (uint)newCols,
                ShiftRow   = newShiftRow,
                ShiftCol   = newShiftCol,
                OffsetRow  = (uint)offsetRow,
                OffsetCol  = (uint)offsetCol,
                Rows       = (uint)sliceRows,
                Cols       = (uint)sliceCols,
                Multipliers = { Capacity = sliceRows * sliceCols }
            };

            for (var r = 0; r < sliceRows; r++)
            for (var c = 0; c < sliceCols; c++)
            {
                var m = MapMultiplier(offsetRow + r, offsetCol + c, newShiftRow, newShiftCol);
                delta.Multipliers.Add(m.ToProto());
            }

            return delta;
        }

        /// <summary>
        /// Translates an <paramref name="absoluteRow"/> / <paramref name="absoluteCol"/>
        /// into the repeating 15 × 15 kernel coordinates and returns the correct
        /// multiplier.
        /// </summary>
        private static MultiplierType MapMultiplier(int absoluteRow, int absoluteCol, int shiftRow, int shiftCol)
        {
            int relativeRow = absoluteRow - shiftRow;
            int relativeCol = absoluteCol - shiftCol;

            int kernelRow = Fold(relativeRow);
            int kernelCol = Fold(relativeCol);

            var raw = Kernel[kernelRow, kernelCol];

            bool isFirstKernel =
                   relativeRow is >= 0 and < KernelSize &&
                   relativeCol is >= 0 and < KernelSize;

            return !isFirstKernel && kernelRow == 7 && kernelCol == 7
                ? None
                : raw;

            static int Fold(int x) =>
                x is >= 0 and < RepeatStart
                    ? x
                    : RepeatStart + (x - RepeatStart).Mod(RepeatPeriod);
        }
    }
}