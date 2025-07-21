using System.Runtime.CompilerServices;

namespace ScrabbleSharp.Engine.Services.MoveGeneration;

/// <summary>
///     A data structure that stores pre-computed "cross-check" information for a Scrabble board.
/// </summary>
/// <remarks>
///     For each empty square on the board, this matrix stores a bitmask representing the set of letters
///     that can be legally placed there to form a valid perpendicular word (a "cross-word").
///     This avoids re-validating cross-words repeatedly during move generation.
/// </remarks>
public sealed class CrossCheckMatrix(uint[,] horizontalMasks, uint[,] verticalMasks)
{
    private const uint AllLettersMask = (1u << 26) - 1;

    /// <summary>
    ///     Masks for forming vertical cross-words (used when the main move is horizontal).
    /// </summary>
    public readonly uint[,] HorizontalMasks = horizontalMasks;

    /// <summary>
    ///     Masks for forming horizontal cross-words (used when the main move is vertical).
    /// </summary>
    public readonly uint[,] VerticalMasks = verticalMasks;

    /// <summary>
    ///     Gets a mask representing all 26 letters, used for squares where any letter is valid.
    /// </summary>
    public static uint FullMask => AllLettersMask;

    /// <summary>
    ///     Checks if placing a given letter at a specified position is legal according to the pre-computed cross-check masks.
    /// </summary>
    /// <param name="row">The row of the square.</param>
    /// <param name="col">The column of the square.</param>
    /// <param name="letter">The letter to check.</param>
    /// <param name="mainIsHorizontal">Whether the main move being generated is horizontal.</param>
    /// <returns><c>true</c> if the placement is legal; otherwise, <c>false</c>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsAllowed(int row, int col, char letter, bool mainIsHorizontal)
    {
        var masks = mainIsHorizontal ? HorizontalMasks : VerticalMasks;
        var bit = 1 << (letter - 'A');
        return (masks[row, col] & (uint)bit) != 0;
    }
}