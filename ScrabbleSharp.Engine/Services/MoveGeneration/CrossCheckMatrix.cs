using System.Runtime.CompilerServices;

namespace ScrabbleSharp.Engine.Services.MoveGeneration;

/// <summary>
///     Constant‑time lookup table telling whether a given letter
///     may be placed at (row,col) without violating perpendicular
///     cross‑word constraints.
/// </summary>
public sealed class CrossCheckMatrix(uint[,] horizontalMasks, uint[,] verticalMasks)
{
    private const uint AllLettersMask = (1u << 26) - 1;

    // When the *main* word is horizontal we need to validate
    // the vertical cross; hence HorizontalMasks deals with
    // horizontal main orientation (vertical cross).
    public readonly uint[,] HorizontalMasks = horizontalMasks;
    public readonly uint[,] VerticalMasks = verticalMasks;

    /// <summary>Bit‑flag for <c>A–Z</c> allowed everywhere (single‑letter cross or empty both sides).</summary>
    public static uint FullMask => AllLettersMask;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsAllowed(int row, int col, char letter, bool mainIsHorizontal)
    {
        var masks = mainIsHorizontal ? HorizontalMasks : VerticalMasks;
        var bit = 1 << (letter - 'A');
        return (masks[row, col] & (uint)bit) != 0;
    }
}