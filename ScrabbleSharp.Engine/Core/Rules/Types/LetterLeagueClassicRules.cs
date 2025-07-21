using ScrabbleSharp.Engine.Core.Rules.Scoring;

namespace ScrabbleSharp.Engine.Core.Rules.Types;

/// <summary>
///     Implements the classic game rules for Letter League, using its specific tile scores.
/// </summary>
public class LetterLeagueClassicRules : GameRulesBase
{
    /// <inheritdoc />
    public override int GetBaseLetterScore(char letter)
    {
        return LetterLeagueTileScores.Table[letter];
    }
}