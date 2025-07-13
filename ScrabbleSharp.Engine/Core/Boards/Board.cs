using ScrabbleSharp.Engine.Core.Boards.Interfaces;
using ScrabbleSharp.Engine.Core.Rules.Interfaces;
using ScrabbleSharp.Engine.Core.Tiles;

namespace ScrabbleSharp.Engine.Core.Boards;

/// <summary>
///     Represents the game board, containing a grid of squares.
/// </summary>
public sealed class Board
{
    private readonly IBoardLayout _layout;

    /// <summary>
    ///     The grid of squares that make up the board.
    /// </summary>
    public readonly Square[,] Grid;

    /// <summary>Column index of the permanent start square (★).</summary>
    public int OriginCol;

    /// <summary>Row index of the permanent start square (★).</summary>
    public int OriginRow;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Board" /> class with a specified layout and game rules.
    /// </summary>
    /// <param name="layout">The layout defining the board's dimensions and multipliers.</param>
    /// <param name="rules">The game rules to be applied.</param>
    /// <exception cref="ArgumentNullException">Thrown if layout or rules are null.</exception>
    public Board(IBoardLayout layout, IGameRules rules)
    {
        _layout = layout ?? throw new ArgumentNullException(nameof(layout));
        Rules = rules ?? throw new ArgumentNullException(nameof(rules));
        Grid = new Square[layout.Rows, layout.Cols];

        for (var row = 0; row < layout.Rows; row++)
        for (var col = 0; col < layout.Cols; col++)
        {
            Grid[row, col] = new Square();
            Grid[row, col].SetMultiplier(layout.GetMultiplier(row, col));
        }

        OriginRow = layout.OriginRow;
        OriginCol = layout.OriginCol;
    }

    /// <summary>
    ///     Gets the game rules associated with this board.
    /// </summary>
    public IGameRules Rules { get; }

    /// <summary>
    ///     Gets the number of rows on the board.
    /// </summary>
    public int Rows => Grid.GetLength(0);

    /// <summary>
    ///     Gets the number of columns on the board.
    /// </summary>
    public int Cols => Grid.GetLength(1);

    /// <summary>
    ///     Gets the square at the specified row and column.
    /// </summary>
    /// <param name="row">The row index.</param>
    /// <param name="column">The column index.</param>
    /// <returns>The <see cref="Square" /> at the specified coordinates.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the coordinates are outside the board's bounds.</exception>
    public Square GetSquare(int row, int column)
    {
        var isSquareOutOfBounds = row < 0 || column < 0 || row >= Rows || column >= Cols;
        if (isSquareOutOfBounds)
            throw new ArgumentOutOfRangeException($"Square ({row},{column}) is outside the {Rows}x{Cols} board.");

        return Grid[row, column];
    }

    /// <summary>
    ///     Checks if the square at the specified coordinates is empty.
    /// </summary>
    /// <param name="row">The row index.</param>
    /// <param name="column">The column index.</param>
    /// <returns>True if the square is empty; otherwise, false.</returns>
    public bool IsEmpty(int row, int column) => GetSquare(row, column).Letter is null;

    /// <summary>
    ///     Places a letter on a square.
    /// </summary>
    /// <param name="row">The row index.</param>
    /// <param name="column">The column index.</param>
    /// <param name="letter">The character to place.</param>
    /// <param name="isBlank">Indicates if the letter is from a blank tile.</param>
    public void SetLetter(int row, int column, char letter, bool isBlank = false)
    {
        var square = GetSquare(row, column); // Throws if out of range
        square.Letter = char.ToUpperInvariant(letter);
        square.IsBlank = isBlank;

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
    ///     Gets the letter multiplier for a square.
    /// </summary>
    /// <param name="row">The row index.</param>
    /// <param name="column">The column index.</param>
    /// <returns>The letter multiplier value.</returns>
    public int GetLetterMultiplier(int row, int column) => GetSquare(row, column).LetterMultiplier;

    /// <summary>
    ///     Gets the word multiplier for a square.
    /// </summary>
    /// <param name="row">The row index.</param>
    /// <param name="column">The column index.</param>
    /// <returns>The word multiplier value.</returns>
    public int GetWordMultiplier(int row, int column) => GetSquare(row, column).WordMultiplier;
}