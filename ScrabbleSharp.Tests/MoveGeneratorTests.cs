using ScrabbleSharp.Engine.Core.Boards.Interfaces;
using ScrabbleSharp.Engine.Core.Boards.Types;
using ScrabbleSharp.Engine.Core.Dictionary;
using ScrabbleSharp.Engine.Core.Rules.Interfaces;
using ScrabbleSharp.Engine.Core.Rules.Types;
using ScrabbleSharp.Engine.Services.MoveGeneration;

namespace ScrabbleSharp.Tests;

[TestFixture]
public class MoveGeneratorTests
{
    private MoveGenerator _moveGenerator;
    private IGameRules _rules;
    private IBoardLayout _layout;
    private DictionaryTrie _dictionary;

    [SetUp]
    public void SetUp()
    {
        _moveGenerator = new MoveGenerator(null); // Logger is optional
        _rules = new ScrabbleClassicRules();
        _layout = new ScrabbleLayout();
        _dictionary = TestHelper.CreateTestTrie(new[] { "HI", "HIT", "HITS", "FIT", "FITS", "IT", "IS", "XI" });
    }

    [Test]
    public void GenerateAllMoves_FirstMove_MustCrossCenter()
    {
        var board = TestHelper.CreateBoard("", _layout, _rules);
        var moves = _moveGenerator.GenerateAllMoves("HIT", board, _dictionary, _rules);

        // With rack "HIT", the generator can form "HI", "HIT", and "IT".
        var expectedWords = new[] { "HI", "HIT", "IT" };
        var actualWords = moves.Select(m => m.Word).Distinct().ToList();

        // Check that moves were found.
        Assert.That(moves, Is.Not.Empty, "No moves were generated.");

        // Check that the generated words are a subset of what's possible.
        CollectionAssert.IsSubsetOf(actualWords, expectedWords);

        // Ensure ALL generated moves for the first turn correctly cross the center square (7,7).
        Assert.That(moves.All(m =>
            (m.StartRow <= 7 && m.StartRow + (m.IsHorizontal ? 0 : m.Word.Length - 1) >= 7) &&
            (m.StartCol <= 7 && m.StartCol + (m.IsHorizontal ? m.Word.Length - 1 : 0) >= 7)
        ), "A generated first move did not cross the center square (7,7).");
    }

    [Test]
    public void GenerateAllMoves_SubsequentMove_FindsAllValidTouchingMoves()
    {
        // The dictionary contains "HI", "HIT", and "IT".
        // Place "HI" on the board at (0,0) and (0,1).
        var board = TestHelper.CreateBoard("HI", _layout, _rules);
        var moves = _moveGenerator.GenerateAllMoves("T", board, _dictionary, _rules);

        // Two moves should be found:
        // 1. Placing 'T' at (0,2) to form "HIT" horizontally.
        // 2. Placing 'T' at (1,1) to form "IT" vertically.
        Assert.That(moves.Count, Is.EqualTo(2));

        // Verify that both expected moves are in the results.
        var moveWords = moves.Select(m => m.Word).ToList();
        CollectionAssert.AreEquivalent(new[] { "HIT", "IT" }, moveWords);

        Assert.Multiple(() =>
        {
            // More specific checks for each move
            Assert.That(moves.Any(m => m.Word == "HIT" && m.StartRow == 0 && m.StartCol == 0 && m.IsHorizontal), Is.True, "Did not find horizontal move for HIT.");
            Assert.That(moves.Any(m => m.Word == "IT" && m.StartRow == 0 && m.StartCol == 1 && !m.IsHorizontal), Is.True, "Did not find vertical move for IT.");
        });
    }

    [Test]
    public void GenerateAllMoves_WithBlank_GeneratesValidWords()
    {
        var smallDict = TestHelper.CreateTestTrie(new[] { "CAT", "BAT", "RAT" });
        var board = TestHelper.CreateBoard("..A.", _layout, _rules); // A at (0,2)
        var moves = _moveGenerator.GenerateAllMoves("T*", board, smallDict, _rules);

        var words = moves.Select(m => m.Word).ToList();
        Assert.That(words, Contains.Item("CAT"));
        Assert.That(words, Contains.Item("BAT"));
        Assert.That(words, Contains.Item("RAT"));

        // Check that the blank was used correctly
        var catMove = moves.First(m => m.Word == "CAT");
        var blankTile = catMove.Tiles.First(t => t.IsBlank);
        Assert.That(blankTile.Letter, Is.EqualTo('C'));
    }

    [Test]
    public void GenerateAllMoves_CrossCheckPreventsInvalidMove()
    {
        // GOAL: Verify that placing 'S' to form "IS" is found, but placing 'X'
        // to form "IX" (which is not in the dictionary) is correctly rejected.

        var dict = TestHelper.CreateTestTrie(["HI", "IS"]);

        // Create a board with a vertical "HI" word.
        // H is at (5,5), I is at (6,5).
        var boardString =
            ".....\n" +
            ".....\n" +
            ".....\n" +
            ".....\n" +
            ".....\n" +
            ".....H\n" + // H at (5,5)
            ".....I"; // I at (6,5)
        var board = TestHelper.CreateBoard(boardString, _layout, _rules);

        // With a rack of "SX", the generator should find an anchor at (6,6)
        // and form the word "IS" horizontally. It should not form "IX".
        var moves = _moveGenerator.GenerateAllMoves("SX", board, dict, _rules);

        var words = moves.Select(m => m.Word).ToHashSet();

        // Assert that the invalid word "IX" was NOT generated.
        Assert.That(words, Does.Not.Contain("IX"));

        // Assert that the valid word "IS" WAS generated.
        Assert.That(words, Does.Contain("IS"));
    }
}