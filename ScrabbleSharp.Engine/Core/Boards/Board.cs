using ScrabbleSharp.Engine.Core.Boards.Interfaces;
using ScrabbleSharp.Engine.Core.Rules.Interfaces;
using ScrabbleSharp.Engine.Core.Tiles;

namespace ScrabbleSharp.Engine.Core.Boards;

/// <summary>
///     Represents the Scrabble game board, consisting of a grid of squares.
/// </summary>
/// <remarks>
///     This class manages the state of the board, including placed tiles and multipliers,
///     and interacts with game rules when tiles are placed.
/// </remarks>
public sealed class Board
{
    /// <summary>
    ///     The 2D array representing the grid of squares on the board.
    /// </summary>
    private readonly Square[,] _grid;

    private readonly IBoardLayout _layout;

    /// <summary>
    ///     The column index of the board's origin (center square), adjusted for expansions.
    /// </summary>
    public readonly int OriginCol;

    /// <summary>
    ///     The row index of the board's origin (center square), adjusted for expansions.
    /// </summary>
    public readonly int OriginRow;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Board" /> class with a specified layout and game rules.
    /// </summary>
    /// <param name="layout">The layout defining the board's dimensions and multiplier placements.</param>
    /// <param name="rules">The game rules to apply to the board.</param>
    /// <exception cref="ArgumentNullException">Thrown if layout or rules are null.</exception>
    public Board(IBoardLayout layout, IGameRules rules)
    {
        _layout = layout ?? throw new ArgumentNullException(nameof(layout));
        Rules = rules ?? throw new ArgumentNullException(nameof(rules));
        _grid = new Square[layout.Rows, layout.Cols];

        for (var row = 0; row < layout.Rows; row++)
        for (var col = 0; col < layout.Cols; col++)
        {
            _grid[row, col] = new Square();
            _grid[row, col].SetMultiplier(layout.GetMultiplier(row, col));
        }

        OriginRow = layout.OriginRow;
        OriginCol = layout.OriginCol;
    }

    /// <summary>
    ///     Gets the game rules associated with this board.
    /// </summary>
    public IGameRules Rules { get; }

    /// <summary>
    ///     Gets the total number of rows on the board.
    /// </summary>
    public int Rows => _grid.GetLength(0);

    /// <summary>
    ///     Gets the total number of columns on the board.
    /// </summary>
    public int Cols => _grid.GetLength(1);

    /// <summary>
    ///     Retrieves the square at the specified row and column.
    /// </summary>
    /// <param name="row">The row index.</param>
    /// <param name="column">The column index.</param>
    /// <returns>The <see cref="Square" /> at the given coordinates.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the coordinates are outside the board's bounds.</exception>
    public Square GetSquare(int row, int column)
    {
        var isSquareOutOfBounds = row < 0 || column < 0 || row >= Rows || column >= Cols;
        if (isSquareOutOfBounds)
            throw new ArgumentOutOfRangeException($"Square ({row},{column}) is outside the {Rows}x{Cols} board.");

        return _grid[row, column];
    }

    /// <summary>
    ///     Checks if the square at the specified coordinates is empty (has no tile).
    /// </summary>
    /// <param name="row">The row index.</param>
    /// <param name="column">The column index.</param>
    /// <returns><c>true</c> if the square is empty; otherwise, <c>false</c>.</returns>
    public bool IsEmpty(int row, int column) => GetSquare(row, column).Letter is null;

    /// <summary>
    ///     Places a letter tile on the board at the specified coordinates.
    /// </summary>
    /// <param name="row">The row index.</param>
    /// <param name="column">The column index.</param>
    /// <param name="letter">The letter to place.</param>
    /// <param name="isBlank">Indicates if the tile is a blank tile.</param>
    public void SetLetter(int row, int column, char letter, bool isBlank = false)
    {
        var square = GetSquare(row, column); // Throws if out of range
        square.Letter = char.ToUpperInvariant(letter);
        square.IsBlank = isBlank;

        // Notify the rules engine that a tile has been placed, allowing it to update scores and multipliers.
        Rules.OnTilePlaced(this, row, column, square.Letter.Value, square.IsBlank);
    }

    /// <summary>
    ///     Sets the multiplier for a specific square.
    /// </summary>
    /// <param name="row">The row index.</param>
    /// <param name="column">The column index.</param>
    /// <param name="multiplier">The multiplier type to set.</param>
    public void SetMultiplier(int row, int column, MultiplierType multiplier)
    {
        GetSquare(row, column).SetMultiplier(multiplier); // Throws if out of range
    }

    /// <summary>
    ///     Gets the letter multiplier for the square at the specified coordinates.
    /// </summary>
    /// <param name="row">The row index.</param>
    /// <param name="column">The column index.</param>
    /// <returns>The letter multiplier value (e.g., 2 for Double Letter).</returns>
    public int GetLetterMultiplier(int row, int column) => GetSquare(row, column).LetterMultiplier;

    /// <summary>
    ///     Gets the word multiplier for the square at the specified coordinates.
    /// </summary>
    /// <param name="row">The row index.</param>
    /// <param name="column">The column index.</param>
    /// <returns>The word multiplier value (e.g., 2 for Double Word).</returns>
    public int GetWordMultiplier(int row, int column) => GetSquare(row, column).WordMultiplier;
}