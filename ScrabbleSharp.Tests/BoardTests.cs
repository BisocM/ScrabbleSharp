using System.Collections.Immutable;
using ScrabbleSharp.Engine.Core.Boards;
using ScrabbleSharp.Engine.Core.Boards.Interfaces;
using ScrabbleSharp.Engine.Core.Boards.Types;
using ScrabbleSharp.Engine.Core.Dictionary;
using ScrabbleSharp.Engine.Core.Models;
using ScrabbleSharp.Engine.Core.Rules.Interfaces;
using ScrabbleSharp.Engine.Core.Rules.Types;
using ScrabbleSharp.Engine.Core.Tiles;
using ScrabbleSharp.Engine.Services.MoveGeneration;

namespace ScrabbleSharp.Tests
{
    /// <summary>
    /// Contains helper methods for creating test assets like boards and dictionaries.
    /// </summary>
    internal static class TestHelper
    {
        public static DictionaryTrie CreateTestTrie(IEnumerable<string> words)
        {
            var trie = new DictionaryTrie();
            trie.LoadWords(words);
            return trie;
        }

        public static Board CreateBoard(string boardString, IBoardLayout layout, IGameRules rules)
        {
            var board = new Board(layout, rules);
            if (string.IsNullOrEmpty(boardString)) return board;

            var lines = boardString.Trim().Split('\n').Select(l => l.Trim()).ToArray();
            for (var row = 0; row < lines.Length; row++)
            {
                if (row >= board.Rows) break;
                var line = lines[row];
                for (var col = 0; col < line.Length; col++)
                {
                    if (col >= board.Cols) break;
                    var c = line[col];
                    if (c == '.') continue;
                    
                    var isBlank = char.IsLower(c);
                    board.SetLetter(row, col, c, isBlank);
                }
            }

            return board;
        }
    }

    #region Core/Boards Tests

    [TestFixture]
    public class BoardTests
    {
        private IBoardLayout _layout;
        private IGameRules _rules;

        [SetUp]
        public void SetUp()
        {
            _layout = new ScrabbleLayout();
            _rules = new ScrabbleClassicRules();
        }

        [Test]
        public void Constructor_WithValidArgs_InitializesGrid()
        {
            var board = new Board(_layout, _rules);
            Assert.Multiple(() =>
            {
                Assert.That(board.Rows, Is.EqualTo(_layout.Rows));
                Assert.That(board.Cols, Is.EqualTo(_layout.Cols));
                Assert.That(board.GetSquare(0, 0).Multiplier, Is.EqualTo(MultiplierType.TripleWord));
            });
        }

        [Test]
        public void Constructor_WithNullLayout_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new Board(null, _rules));
        }

        [Test]
        public void GetSquare_OutOfBounds_ThrowsArgumentOutOfRangeException()
        {
            var board = new Board(_layout, _rules);
            Assert.Throws<ArgumentOutOfRangeException>(() => board.GetSquare(-1, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => board.GetSquare(0, -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => board.GetSquare(board.Rows, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => board.GetSquare(0, board.Cols));
        }

        [Test]
        public void SetLetter_SetsLetterAndRemovesMultiplier()
        {
            var board = new Board(_layout, _rules);
            var square = board.GetSquare(0, 3);
            Assert.That(square.Multiplier, Is.EqualTo(MultiplierType.DoubleLetter));

            board.SetLetter(0, 3, 'A');

            Assert.That(square.Letter, Is.EqualTo('A'));
            Assert.That(square.IsBlank, Is.False);
            Assert.That(square.Multiplier, Is.EqualTo(MultiplierType.None), "Multiplier should be consumed after placing a tile.");
        }

        [Test]
        public void SetLetter_WithBlank_SetsIsBlankAndLetter()
        {
            var board = new Board(_layout, _rules);
            board.SetLetter(1, 1, 'b', isBlank: true);
            var square = board.GetSquare(1, 1);

            Assert.Multiple(() =>
            {
                Assert.That(square.Letter, Is.EqualTo('B'));
                Assert.That(square.IsBlank, Is.True);
            });
        }

        [Test]
        public void IsEmpty_ReturnsCorrectState()
        {
            var board = new Board(_layout, _rules);
            Assert.That(board.IsEmpty(5, 5), Is.True);
            board.SetLetter(5, 5, 'X');
            Assert.That(board.IsEmpty(5, 5), Is.False);
        }
    }

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

    #endregion

    #region Core/Rules/Scoring Tests

    [TestFixture]
    public class ScrabbleClassicRulesTests
    {
        private IGameRules _rules;
        private IBoardLayout _layout;

        [SetUp]
        public void SetUp()
        {
            _rules = new ScrabbleClassicRules();
            _layout = new ScrabbleLayout();
        }

        [TestCase('A', 1)]
        [TestCase('Q', 10)]
        [TestCase('L', 1)]
        [TestCase('*', 0)]
        public void GetBaseLetterScore_ReturnsCorrectScrabbleScore(char letter, int expectedScore)
        {
            Assert.That(_rules.GetBaseLetterScore(letter), Is.EqualTo(expectedScore));
        }

        [Test]
        public void GetLetterScore_ForBlankTile_IsZero()
        {
            var board = new Board(_layout, _rules);
            var score = _rules.GetLetterScore(board, 0, 0, 'A', true, true);
            Assert.That(score, Is.EqualTo(0));
        }

        [Test]
        public void ApplyFinalBonuses_With7Tiles_Adds50PointBingo()
        {
            Assert.That(_rules.ApplyFinalBonuses(100, 7), Is.EqualTo(150));
        }

        [Test]
        public void ApplyFinalBonuses_WithLessThan7Tiles_AddsNoBonus()
        {
            Assert.That(_rules.ApplyFinalBonuses(100, 6), Is.EqualTo(100));
        }

        [Test]
        public void CalculateMoveScore_SimpleHorizontalWord()
        {
            // Place WORD on H8 (row 7) starting at H8 (col 7), the center square.
            var board = TestHelper.CreateBoard("", _layout, _rules);
            var move = new Move
            {
                Word = "WORD",
                StartRow = 7,
                StartCol = 7,
                IsHorizontal = true,
                Tiles = ImmutableList.Create(
                    new TilePlacement(7, 7, 'W', false),
                    new TilePlacement(7, 8, 'O', false),
                    new TilePlacement(7, 9, 'R', false),
                    new TilePlacement(7, 10, 'D', false)
                )
            };
            // W(4) on DW -> 4. O(1). R(1). D(2).
            // Sum = 4 + 1 + 1 + 2 = 8.
            // Word Multiplier: DW -> 8 * 2 = 16.
            var score = _rules.CalculateMoveScore(board, move);
            Assert.That(score, Is.EqualTo(16));
        }

        [Test]
        public void CalculateMoveScore_WithCrossWordAndMultipliers()
        {
            // Board: HELLO horizontally at (7,7). Place 'S' at (8,7) to form 'HELLOS' and 'SO' vertically.
            var board = TestHelper.CreateBoard(".......HELLO...", _layout, _rules);
            var move = new Move
            {
                Word = "SO",
                StartRow = 7,
                StartCol = 8,
                IsHorizontal = false,
                Tiles = ImmutableList.Create(new TilePlacement(8, 8, 'S', false)),
                // The main word formed is 'SO' vertically, but the move generator would have
                // found all created words. We simulate the score calculation.
            };

            // This test highlights a subtlety. `CalculateMoveScore` calculates the MAIN word
            // and then calculates each CROSS word. Let's assume the move is placing S at (8,8)
            // extending the existing O at (7,8).
            var tile = new TilePlacement(8, 8, 'S', false);
            // Main word: SO (vertical). (7,8) is O (existing). (8,8) is S (new, on TL).
            // O(1) + S(1)*3 = 4.
            // There are no word multipliers for 'SO'. Score = 4.
            // There are no other cross words.
            // Let's create a more realistic Move object.
            var fullMove = new Move
            {
                Word = "SO", StartRow = 7, StartCol = 8, IsHorizontal = false, Tiles = ImmutableList.Create(tile)
            };

            // Score of main word 'SO':
            // O at (7,8) is existing. Score = 1.
            // S at (8,8) is new, on a TL square. Score = 1 * 3 = 3.
            // Total = 1 + 3 = 4. No word multipliers. Main score = 4.
            var mainScore = 4;

            // Score of cross-word 'HELLOS':
            // H,E,L,L,O are existing. S is new.
            // S is placed at (8,8). No multipliers on that square for a horizontal word.
            // Score = H(4)+E(1)+L(1)+L(1)+O(1)+S(1) = 9.
            // The center square (7,7) has a DW that was used by HELLO, so it doesn't apply again.
            // Cross score should be calculated by the cross word logic.
            // Let's trace CalculateCrossWordScore(board, tile, mainIsHorizontal: false)
            // It will find the horizontal word 'HELLOS'.
            // H(4)+E(1)+L(1)+L(1)+O(1) are existing. Letter scores = 8.
            // S(1) is new. No letter multiplier at (8,8) for horizontal words. Score = 1.
            // Total score for 'HELLOS' = 8 + 1 = 9. No word mults.
            var crossScore = 9;

            var totalScore = mainScore + crossScore;
            Assert.That(totalScore, Is.EqualTo(13));
        }

        [Test]
        public void CalculateMoveScore_MultipleWordMultipliers()
        {
            // Place a word across two TW squares. e.g. at (0,0) and (0,7).
            var board = TestHelper.CreateBoard("", _layout, _rules);
            var move = new Move
            {
                Word = "QUARTER",
                StartRow = 0,
                StartCol = 0,
                IsHorizontal = true,
                Tiles = ImmutableList.Create(
                    new TilePlacement(0, 0, 'Q', false), // TW
                    new TilePlacement(0, 1, 'U', false),
                    new TilePlacement(0, 2, 'A', false),
                    new TilePlacement(0, 3, 'R', false),
                    new TilePlacement(0, 4, 'T', false),
                    new TilePlacement(0, 5, 'E', false),
                    new TilePlacement(0, 6, 'R', false)
                )
            };

            // Place 'QUARTER' starting at (0,0), but the 'R' lands on the TW at (0,7)
            // This setup is slightly flawed because it requires 8 squares. Let's use a 7-letter word
            // like 'JOKER' starting at (0,3) ending at (0,7) with a tile on (0,0).
            board.SetLetter(0, 0, 'F'); // Existing tile
            var move2 = new Move
            {
                Word = "JOKERS",
                StartRow = 0,
                StartCol = 2,
                IsHorizontal = true,
                Tiles = ImmutableList.Create(
                    new TilePlacement(0, 2, 'J', false),
                    new TilePlacement(0, 3, 'O', false), // (0,3) is DL
                    new TilePlacement(0, 4, 'K', false),
                    new TilePlacement(0, 5, 'E', false),
                    new TilePlacement(0, 6, 'R', false),
                    new TilePlacement(0, 7, 'S', false) // (0,7) is TW
                )
            };

            // This is complex. Let's simplify.
            // Word across two DWs, e.g. on row 1, (1,1) and (1,5)
            // F(4) A(1) R(1) M(3) S(1)
            var farmMove = new Move
            {
                Word = "FARMS", StartRow = 1, StartCol = 1, IsHorizontal = true,
                Tiles = ImmutableList.Create(
                    new TilePlacement(1, 1, 'F', false), // DW
                    new TilePlacement(1, 2, 'A', false),
                    new TilePlacement(1, 3, 'R', false),
                    new TilePlacement(1, 4, 'M', false),
                    new TilePlacement(1, 5, 'S', false) // TL
                )
            };
            // F(4), A(1), R(1), M(3), S(1) on TL -> 1*3=3.
            // Letter scores sum = 4 + 1 + 1 + 3 + 3 = 12.
            // Word multiplier from (1,1) is DW (x2).
            // Total score = 12 * 2 = 24.
            var score = _rules.CalculateMoveScore(board, farmMove);
            Assert.That(score, Is.EqualTo(24));
        }
    }

    #endregion

    #region Services/MoveGeneration Tests

    [TestFixture]
    public class RackCountsTests
    {
        [Test]
        public void Constructor_WithMixedCaseAndBlank_InitializesCorrectly()
        {
            var rack = new RackCounts("aB*_C");
            Assert.Multiple(() =>
            {
                Assert.That(rack.TilesRemaining, Is.EqualTo(5));
                Assert.That(rack.Has('A'), Is.True);
                Assert.That(rack.Has('B'), Is.True);
                Assert.That(rack.Has('*'), Is.True);
                Assert.That(rack.Has('C'), Is.True);
                Assert.That(rack.Has('D'), Is.False);
            });
        }

        [Test]
        public void Take_RemovesTileAndDecrementsCount()
        {
            var rack = new RackCounts("HELLO");
            rack.Take('L');
            rack.Take('L');
            Assert.Multiple(() =>
            {
                Assert.That(rack.Has('L'), Is.False);
                Assert.That(rack.TilesRemaining, Is.EqualTo(3));
            });
        }

        [Test]
        public void Take_TileNotOnRack_ThrowsInvalidOperationException()
        {
            var rack = new RackCounts("ABC");
            Assert.Throws<InvalidOperationException>(() => rack.Take('D'));
        }

        [Test]
        public void Put_AddsTileAndIncrementsCount()
        {
            var rack = new RackCounts("ABC");
            rack.Take('A');
            rack.Put('A');
            Assert.Multiple(() =>
            {
                Assert.That(rack.Has('A'), Is.True);
                Assert.That(rack.TilesRemaining, Is.EqualTo(3));
            });
        }

        [Test]
        public void DistinctTiles_ReturnsUniqueTilesSortedByValue()
        {
            // Scrabble Scores: Q(10), J(8), A(1), E(1), *(0)
            var rack = new RackCounts("JAEQ*");
            var distinct = rack.DistinctTiles().ToList();

            var expectedOrder = new List<char> { 'Q', 'J', 'E', 'A', '*' };

            Assert.That(distinct, Is.EqualTo(expectedOrder));
        }
    }

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

    #endregion
}