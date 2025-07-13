namespace ScrabbleSharp.Gateway.Configuration;

public sealed class ValidationSettings
{
    public int MaxRackSize { get; set; } = 7;
    public int MaxWildcards { get; set; } = 3;
    public int MaxBoardDimension { get; set; } = 40;
    public int MaxBandsPerDirection { get; set; } = 4;
}