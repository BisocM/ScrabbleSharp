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
}