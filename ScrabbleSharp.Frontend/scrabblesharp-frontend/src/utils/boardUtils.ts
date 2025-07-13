import type { Board } from "@/app/board/types";

/**
 * Converts the 2D board array into a multi-line string format expected by the backend.
 * - Empty cells are represented by '.'.
 * - Placed letters are uppercase.
 * - Blank tiles are represented by their chosen letter in lowercase.
 *
 * @param board The board state to convert.
 * @returns A string representation of the board.
 */
export function boardToString(board: Board): string {
    return board
        .map(row =>
            row
                .map(cell =>
                    cell
                        ? cell.isBlank ? cell.letter.toLowerCase() : cell.letter.toUpperCase()
                        : "."
                )
                .join("")
        )
        .join("\n");
}

/**
 * Creates a shallow copy of the board array and its rows. This is sufficient
 * for immutability in Redux as long as the cell objects themselves are replaced,
 * not mutated.
 *
 * @param board The board to clone.
 * @returns A new 2D array with the same cells.
 */
export function cloneBoard(board: Board): Board {
    return board.map(row => row.slice());
}