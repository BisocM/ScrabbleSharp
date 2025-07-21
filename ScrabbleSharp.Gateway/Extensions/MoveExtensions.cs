using ScrabbleSharp.Contracts.Protos;

namespace ScrabbleSharp.Gateway.Extensions;

/// <summary>
///     Provides extension methods for converting between engine and Protobuf model types.
/// </summary>
public static class MoveExtensions
{
    /// <summary>
    ///     Converts an engine <see cref="Engine.Core.Models.Move" /> to its Protobuf <see cref="Move" /> representation.
    /// </summary>
    /// <param name="engineMove">The engine move object to convert.</param>
    /// <returns>The corresponding Protobuf move object.</returns>
    public static Move ToProto(this Engine.Core.Models.Move engineMove)
    {
        return new Move
        {
            Word = engineMove.Word,
            StartRow = (uint)engineMove.StartRow,
            StartCol = (uint)engineMove.StartCol,
            Horizontal = engineMove.IsHorizontal,
            Score = (uint)engineMove.Score,
            Definition = engineMove.Defintion
        };
    }
}