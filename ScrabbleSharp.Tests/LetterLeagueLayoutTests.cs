using ScrabbleSharp.Engine.Core.Boards.Types;
using ScrabbleSharp.Engine.Core.Tiles;

namespace ScrabbleSharp.Tests;

[TestFixture]
public class LetterLeagueLayoutTests
{
    private LetterLeagueLayout _layout;

    [SetUp]
    public void SetUp()
    {
        _layout = new LetterLeagueLayout();
    }

    [Test]
    public void InitialState_Is15x15WithCorrectOrigin()
    {
        Assert.Multiple(() =>
        {
            Assert.That(_layout.Rows, Is.EqualTo(15));
            Assert.That(_layout.Cols, Is.EqualTo(15));
            Assert.That(_layout.OriginRow, Is.EqualTo(7));
            Assert.That(_layout.OriginCol, Is.EqualTo(7));
        });
    }

    [Test]
    public void GetMultiplier_OnInitialBoard_IsCorrect()
    {
        Assert.Multiple(() =>
        {
            Assert.That(_layout.GetMultiplier(0, 0), Is.EqualTo(MultiplierType.TripleLetter));
            Assert.That(_layout.GetMultiplier(7, 7), Is.EqualTo(MultiplierType.None)); // Center is None
            Assert.That(_layout.GetMultiplier(1, 1), Is.EqualTo(MultiplierType.TripleWord));
        });
    }

    [Test]
    public void TryExpandAt_Up_ExpandsBoardCorrectly()
    {
        var delta = _layout.TryExpandAt(0, 7); // Trigger upward expansion

        Assert.Multiple(() =>
        {
            Assert.That(delta, Is.Not.Null);
            Assert.That(_layout.Rows, Is.EqualTo(15 + 4));
            Assert.That(_layout.Cols, Is.EqualTo(15));
            Assert.That(_layout.OriginRow, Is.EqualTo(7 + 4)); // Origin shifts
        });
        Assert.Multiple(() =>
        {
            Assert.That(delta.ShiftRow, Is.EqualTo(4));
            Assert.That(delta.TotalRows, Is.EqualTo(19));
            Assert.That(delta.OffsetRow, Is.EqualTo(0));
            Assert.That(delta.Rows, Is.EqualTo(4));

            // Test a multiplier in the new section based on folding logic
            // Row 0 in the new grid is absolute row 0. Relative row is 0 - shiftRow(4) = -4.
            // Fold(-4) = 3 + (-4 - 3).Mod(12) = 3 + (-7).Mod(12) = 3 + 5 = 8.
            // The multiplier should be Kernel[8, 7], which is None.
            Assert.That(_layout.GetMultiplier(0, 7), Is.EqualTo(MultiplierType.None));
        });
    }

    [Test]
    public void TryExpandAt_LimitReached_ReturnsNull()
    {
        _layout.SetExpansionLimits(up: 1);
        _layout.TryExpandAt(0, 7); // First expansion is ok
        var delta = _layout.TryExpandAt(0, 7); // Second should fail
        Assert.That(delta, Is.Null);
    }

    [Test]
    public void Reset_RevertsToInitialState()
    {
        _layout.TryExpandAt(0, 7); // Expand once
        _layout.Reset();

        Assert.Multiple(() =>
        {
            Assert.That(_layout.Rows, Is.EqualTo(15));
            Assert.That(_layout.Cols, Is.EqualTo(15));
            Assert.That(_layout.OriginRow, Is.EqualTo(7));
            Assert.That(_layout.OriginCol, Is.EqualTo(7));
        });
    }
}