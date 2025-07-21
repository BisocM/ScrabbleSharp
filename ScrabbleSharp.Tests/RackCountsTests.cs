using ScrabbleSharp.Engine.Services.MoveGeneration;

namespace ScrabbleSharp.Tests;

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