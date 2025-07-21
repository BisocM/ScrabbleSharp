using ScrabbleSharp.Contracts.Protos;
using ScrabbleSharp.Engine.Core.Boards.Interfaces;
using ScrabbleSharp.Engine.Core.Utils;

namespace ScrabbleSharp.Gateway.Extensions;

/// <summary>
///     Provides extension methods for <see cref="IBoardLayout" /> and related interfaces.
/// </summary>
public static class BoardLayoutExtensions
{
    private const int MaxBandsPerDirection = 4;

    /// <summary>
    ///     Applies a specified number of expansion bands to an <see cref="IExpandableBoardLayout" />.
    /// </summary>
    /// <param name="expandableLayout">The layout to expand.</param>
    /// <param name="bands">A tuple containing the number of bands to add in each direction (Up, Down, Left, Right).</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the number of bands in any direction is invalid.</exception>
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
    ///     Gets the coordinates that will trigger an expansion in the specified direction.
    /// </summary>
    /// <param name="layout">The board layout.</param>
    /// <param name="direction">The desired direction of expansion.</param>
    /// <returns>A tuple (row, col) representing the trigger coordinates.</returns>
    public static (int row, int col) GetTriggerCoordinates(this IBoardLayout layout, Direction direction)
    {
        return direction switch
        {
            Direction.Up => (0, layout.Cols / 2),
            Direction.Down => (layout.Rows - 1, layout.Cols / 2),
            Direction.Left => (layout.Rows / 2, 0),
            Direction.Right => (layout.Rows / 2, layout.Cols - 1),
            // Should not be reached for valid directions.
            _ => (layout.Rows / 2, layout.Cols / 2)
        };
    }

    /// <summary>
    ///     Converts an <see cref="IBoardLayout" /> to its Protobuf <see cref="LayoutResponse" /> representation.
    /// </summary>
    /// <param name="layout">The layout to convert.</param>
    /// <returns>A <see cref="LayoutResponse" /> object.</returns>
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