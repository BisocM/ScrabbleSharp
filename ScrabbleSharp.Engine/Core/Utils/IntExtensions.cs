namespace ScrabbleSharp.Engine.Core.Utils;

/// <summary>
///     Provides extension methods for integer types.
/// </summary>
public static class IntExtensions
{
    /// <summary>
    ///     Calculates the true mathematical modulus, which is always non-negative.
    ///     The C# `%` operator can return a negative result, which is undesirable for array indexing.
    /// </summary>
    /// <param name="value">The dividend.</param>
    /// <param name="modulus">The divisor.</param>
    /// <returns>The non-negative remainder of the division.</returns>
    public static int Mod(this int value, int modulus)
    {
        return (value % modulus + modulus) % modulus;
    }
}