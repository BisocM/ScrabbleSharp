using ScrabbleSharp.Contracts.Protos;
using ScrabbleSharp.Engine.Core.Tiles;

namespace ScrabbleSharp.Engine.Core.Utils;

/// <summary>
///     Provides extension methods for converting <see cref="MultiplierType" /> enums.
/// </summary>
public static class MultiplierExtensions
{
    /// <summary>
    ///     Converts a core <see cref="MultiplierType" /> to its Protocol Buffer equivalent.
    /// </summary>
    public static Multiplier ToProto(this MultiplierType value) => value switch
    {
        MultiplierType.DoubleLetter => Multiplier.DoubleLetter,
        MultiplierType.TripleLetter => Multiplier.TripleLetter,
        MultiplierType.QuadrupleLetter => Multiplier.QuadrupleLetter,
        MultiplierType.DoubleWord => Multiplier.DoubleWord,
        MultiplierType.TripleWord => Multiplier.TripleWord,
        MultiplierType.QuadrupleWord => Multiplier.QuadrupleWord,
        _ => Multiplier.None
    };

    /// <summary>
    ///     Converts a Protocol Buffer <see cref="Multiplier" /> to its core engine equivalent.
    /// </summary>
    public static MultiplierType ToCore(this Multiplier value) => value switch
    {
        Multiplier.DoubleLetter => MultiplierType.DoubleLetter,
        Multiplier.TripleLetter => MultiplierType.TripleLetter,
        Multiplier.QuadrupleLetter => MultiplierType.QuadrupleLetter,
        Multiplier.DoubleWord => MultiplierType.DoubleWord,
        Multiplier.TripleWord => MultiplierType.TripleWord,
        Multiplier.QuadrupleWord => MultiplierType.QuadrupleWord,
        _ => MultiplierType.None
    };

    /// <summary>
    ///     Converts a <see cref="MultiplierType" /> to its integer factor (e.g., TripleWord -> 3).
    /// </summary>
    public static int ToInt(this MultiplierType value) => value switch
    {
        MultiplierType.QuadrupleWord => 4,
        MultiplierType.QuadrupleLetter => 4,
        MultiplierType.TripleWord => 3,
        MultiplierType.TripleLetter => 3,
        MultiplierType.DoubleWord => 2,
        MultiplierType.DoubleLetter => 2,
        MultiplierType.None => 1,
        _ => 1
    };
}