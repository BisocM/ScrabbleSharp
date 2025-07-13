/**
 * Represents a single tile placed on the board.
 * `letter` is the character on the tile.
 * `isBlank` is true if the tile was a blank tile.
 */
export interface Cell {
    letter: string;
    isBlank: boolean;
}

/**
 * Represents the game board as a 2D array.
 * Each element is either a `Cell` object or `null` if the square is empty.
 */
export type Board = (Cell | null)[][];

/**
 * Represents the data needed to preview a move on the board,
 * typically when hovering over it in the word list.
 */
export interface MovePreview {
    word: string;
    startRow: number;
    startCol: number;
    horizontal: boolean;
}