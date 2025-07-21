using ScrabbleSharp.Engine.Core.Boards;
using ScrabbleSharp.Engine.Core.Boards.Interfaces;
using ScrabbleSharp.Engine.Core.Rules.Interfaces;

namespace ScrabbleSharp.Engine.Serialization;

/// <summary>
///     Provides static methods for creating and applying board states from a string representation.
/// </summary>
public static class BoardSnapshot
{
    /// <summary>
    ///     Creates a new <see cref="Board" /> instance from a string snapshot.
    /// </summary>
    /// <param name="snapshot">
    ///     The string representation of the board state.
    ///     - Newlines separate rows.
    ///     - Uppercase letters represent standard tiles.
    ///     - Lowercase letters represent blank tiles played as that letter.
    ///     - '.', ' ', or '0' represent empty squares.
    /// </param>
    /// <param name="layout">The board layout to use.</param>
    /// <param name="rules">The game rules to associate with the board.</param>
    /// <returns>A new <see cref="Board" /> instance initialized with the snapshot state.</returns>
    public static Board FromString(string snapshot,
        IBoardLayout layout,
        IGameRules rules)
    {
        var board = new Board(layout, rules);
        Apply(board, snapshot);
        return board;
    }

    /// <summary>
    ///     Applies a string snapshot to an existing <see cref="Board" /> instance.
    /// </summary>
    /// <param name="board">The board to modify.</param>
    /// <param name="snapshot">
    ///     The string representation of the board state to apply.
    ///     - Newlines separate rows.
    ///     - Uppercase letters represent standard tiles.
    ///     - Lowercase letters represent blank tiles played as that letter.
    ///     - '.', ' ', or '0' represent empty squares.
    /// </param>
    public static void Apply(Board board, string snapshot)
    {
        if (string.IsNullOrWhiteSpace(snapshot))
            return;

        var lines = snapshot.Split(
            ['\r', '\n'],
            StringSplitOptions.RemoveEmptyEntries);

        for (var row = 0; row < lines.Length; row++)
        {
            var line = lines[row];

            for (var column = 0; column < line.Length; column++)
            {
                var character = line[column];
                if (character is '.' or ' ' or '0')
                    continue; // This represents an empty square.

                var isBlank = char.IsLower(character);
                var letter = isBlank
                    ? char.ToUpperInvariant(character)
                    : character;

                board.SetLetter(row, column, letter, isBlank);
            }
        }

        if (board.Rules is IBoardInitialiser initialiser)
            initialiser.Initialise(board);
    }
}