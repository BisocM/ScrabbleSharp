namespace ScrabbleSharp.Gateway.Configuration;

/// <summary>
///     Defines validation constraints for incoming requests.
/// </summary>
public sealed class ValidationSettings
{
    /// <summary>
    ///     Gets or sets the maximum number of tiles allowed in a player's rack.
    /// </summary>
    public int MaxRackSize { get; set; } = 7;

    /// <summary>
    ///     Gets or sets the maximum number of wildcard (blank) tiles allowed in a rack.
    /// </summary>
    public int MaxWildcards { get; set; } = 3;

    /// <summary>
    ///     Gets or sets the maximum dimension (rows or columns) for a game board.
    /// </summary>
    public int MaxBoardDimension { get; set; } = 40;

    /// <summary>
    ///     Gets or sets the maximum number of expansion bands allowed per direction for expandable boards.
    /// </summary>
    public int MaxBandsPerDirection { get; set; } = 4;
}