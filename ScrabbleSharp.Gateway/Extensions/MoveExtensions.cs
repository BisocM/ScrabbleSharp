using ScrabbleSharp.Contracts.Protos;

namespace ScrabbleSharp.Gateway.Extensions;

public static class MoveExtensions
{
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