using ScrabbleSharp.Contracts.Protos;
using ScrabbleSharp.Engine.Core.Boards.Interfaces;
using ScrabbleSharp.Engine.Core.Utils;

namespace ScrabbleSharp.Gateway.Extensions;

/// <summary>
///     Provides extension methods for <see cref="IBoardLayout" /> and related interfaces.
/// </summary>
public static class BoardLayoutExtensions
{
    /// <summary>
    ///     Hard upper bound for user-requested expansion: at most four bands
    ///     (4-square rings) per cardinal direction for the lifetime of a board.
    /// </summary>
    private const int MaxBandsPerDirection = 4;

    /// <summary>
    ///     Expands the supplied <see cref="IExpandableBoardLayout" /> by the number
    ///     of bands requested in <paramref name="bands" />, after validating that
    ///     each component is within the allowed range <c>[0, MaxBandsPerDirection]</c>.
    ///     A value outside that range results in <see cref="ArgumentOutOfRangeException" />.
    /// </summary>
    public static void ApplyBands(
        this IExpandableBoardLayout expandableLayout,
        (int Up, int Down, int Left, int Right) bands)
    {
        Validate(bands.Up, nameof(bands.Up));
        Validate(bands.Down, nameof(bands.Down));
        Validate(bands.Left, nameof(bands.Left));
        Validate(bands.Right, nameof(bands.Right));

        for (var i = 0; i < bands.Up; i++)
            expandableLayout.TryExpandAt(0, expandableLayout.Cols / 2);

        for (var i = 0; i < bands.Down; i++)
            expandableLayout.TryExpandAt(expandableLayout.Rows - 1, expandableLayout.Cols / 2);

        for (var i = 0; i < bands.Left; i++)
            expandableLayout.TryExpandAt(expandableLayout.Rows / 2, 0);

        for (var i = 0; i < bands.Right; i++)
            expandableLayout.TryExpandAt(expandableLayout.Rows / 2, expandableLayout.Cols - 1);

        static void Validate(int value, string paramName)
        {
            if (value < 0 || value > MaxBandsPerDirection)
                throw new ArgumentOutOfRangeException(
                    paramName,
                    $"Requested expansion ({value}) exceeds the allowed " +
                    $"range 0–{MaxBandsPerDirection} per direction.");
        }
    }

    /// <summary>
    ///     Gets the coordinates used to trigger an expansion in a given direction.
    /// </summary>
    /// <param name="layout">The board layout.</param>
    /// <param name="direction">The direction of expansion.</param>
    /// <returns>A tuple containing the trigger row and column.</returns>
    public static (int row, int col) GetTriggerCoordinates(this IBoardLayout layout, Direction direction)
    {
        return direction switch
        {
            Direction.Up => (0, layout.Cols / 2),
            Direction.Down => (layout.Rows - 1, layout.Cols / 2),
            Direction.Left => (layout.Rows / 2, 0),
            Direction.Right => (layout.Rows / 2, layout.Cols - 1),
            _ => (layout.Rows / 2, layout.Cols / 2) // Should not be reached
        };
    }

    /// <summary>
    ///     Converts a board layout to its protobuf <see cref="LayoutResponse" /> representation.
    /// </summary>
    /// <param name="layout">The board layout to convert.</param>
    /// <returns>A <see cref="LayoutResponse" /> message.</returns>
    public static LayoutResponse ToLayoutResponse(this IBoardLayout layout)
    {
        var response = new LayoutResponse
        {
            Rows = (uint)layout.Rows,
            Cols = (uint)layout.Cols,
            Expandable = layout is IExpandableBoardLayout
        };

        for (var row = 0; row < layout.Rows; row++)
        for (var column = 0; column < layout.Cols; column++)
            response.Multipliers.Add(layout.GetMultiplier(row, column).ToProto());

        return response;
    }
}