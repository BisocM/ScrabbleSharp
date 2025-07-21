namespace ScrabbleSharp.Engine.Core.Models;

/// <summary>
///     Represents the placement of a single tile on the board.
/// </summary>
/// <param name="Row">The row index of the placement.</param>
/// <param name="Col">The column index of the placement.</param>
/// <param name="Letter">The letter on the tile (or the letter chosen for a blank).</param>
/// <param name="IsBlank">Indicates if the placed tile was a blank.</param>
public sealed record TilePlacement(int Row, int Col, char Letter, bool IsBlank);