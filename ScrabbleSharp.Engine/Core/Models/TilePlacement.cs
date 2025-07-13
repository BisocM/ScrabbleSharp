namespace ScrabbleSharp.Engine.Core.Models;

/// <summary>
///     Represents a single tile being placed on the board.
/// </summary>
/// <param name="Row">The row where the tile is placed.</param>
/// <param name="Col">The column where the tile is placed.</param>
/// <param name="Letter">The letter on the tile (or the letter a blank is representing).</param>
/// <param name="IsBlank">A value indicating whether the tile is a blank.</param>
public sealed record TilePlacement(int Row, int Col, char Letter, bool IsBlank);