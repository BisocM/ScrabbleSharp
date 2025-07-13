using ScrabbleSharp.Engine.Core.Boards;
using ScrabbleSharp.Engine.Core.Boards.Interfaces;
using ScrabbleSharp.Engine.Core.Rules.Interfaces;

namespace ScrabbleSharp.Engine.Serialization;

/// <summary>
///     Provides static methods for creating and applying board state from a string representation.
/// </summary>
public static class BoardSnapshot
{
    /// <summary>
    ///     Creates a new board from a string snapshot.
    /// </summary>
    /// <param name="snapshot">The string representing the board state. Lowercase letters indicate blanks.</param>
    /// <param name="layout">The board layout to use.</param>
    /// <param name="rules">The game rules to apply.</param>
    /// <returns>A new <see cref="Board" /> instance with the state applied.</returns>
    public static Board FromString(string snapshot,
        IBoardLayout layout,
        IGameRules rules)
    {
        var board = new Board(layout, rules);
        Apply(board, snapshot);
        return board;
    }

    /// <summary>
    ///     Applies a string snapshot to an existing board.
    /// </summary>
    /// <param name="board">The board to modify.</param>
    /// <param name="snapshot">The string representing the board state.</param>
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

                // This is the fix. Instead of manually setting properties,
                // we call the board's SetLetter method. This ensures that
                // game rules, like consuming the multiplier, are correctly triggered.
                board.SetLetter(row, column, letter, isBlank);
            }
        }

        if (board.Rules is IBoardInitialiser initialiser)
            initialiser.Initialise(board);
    }
}