using ScrabbleSharp.Engine.Core.Rules.Scoring;

namespace ScrabbleSharp.Engine.Core.Rules.Types;

/// <summary>
///     Implements game rules for classic Scrabble, using standard tile scores.
/// </summary>
public sealed class ScrabbleClassicRules : GameRulesBase
{
    /// <inheritdoc />
    public override int GetBaseLetterScore(char letter)
    {
        return ScrabbleTileScores.Table[letter];
    }
}