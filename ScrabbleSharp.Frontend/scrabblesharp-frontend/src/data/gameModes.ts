/**
 * Defines the structure for information about a specific game mode.
 */
export interface GameModeInfo {
    id: string;
    label: string;
    initialRows: number;
    initialCols: number;
}

/**
 * A constant object holding the configurations for all available game modes.
 * `as const` is used for stricter type inference.
 */
export const GAME_MODES = {
    letterleague_classic: {
        id: "letterleague_classic",
        label: "Letter League Classic",
        initialRows: 15,
        initialCols: 15,
    },
    scrabble_classic: {
        id: "scrabble_classic",
        label: "Scrabble Classic",
        initialRows: 15,
        initialCols: 15,
    },
    scrabble_duel: {
        id: "scrabble_duel",
        label: "Scrabble Duel",
        initialRows: 11,
        initialCols: 11,
    },
    scrabble_super: {
        id: "scrabble_super",
        label: "Super Scrabble",
        initialRows: 21,
        initialCols: 21,
    },
} as const;

// A type representing the valid IDs for game modes, derived from the keys of GAME_MODES.
export type GameModeId = keyof typeof GAME_MODES;

/**
 * Retrieves the information object for a given game mode ID.
 * @param mode The ID of the game mode.
 * @returns The corresponding GameModeInfo object.
 */
export function getModeInfo(mode: GameModeId): GameModeInfo {
    return GAME_MODES[mode];
}