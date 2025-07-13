namespace ScrabbleSharp.Engine.Core.Utils;

/// <summary>
///     Provides extension methods for integer operations.
/// </summary>
public static class IntExtensions
{
    /// <summary>
    ///     Computes the mathematical modulus, which is always non-negative.
    /// </summary>
    /// <param name="value">The integer value.</param>
    /// <param name="modulus">The modulus.</param>
    /// <returns>The non-negative remainder.</returns>
    public static int Mod(this int value, int modulus)
    {
        return (value % modulus + modulus) % modulus;
    }
}