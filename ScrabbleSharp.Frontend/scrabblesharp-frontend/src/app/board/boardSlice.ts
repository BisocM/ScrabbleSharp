import { createSlice, PayloadAction } from "@reduxjs/toolkit";
import type { Board, MovePreview } from "./types";
import type { Move } from "@/api/solverApi";
import { safeGet } from "@/utils/storage";

export const createEmptyBoard = (rows = 15, cols = 15): Board =>
    Array.from({ length: rows }, () => Array.from({ length: cols }, () => null));

export type BandDirection = "up" | "down" | "left" | "right";

export interface BandCounters {
    up: number;
    down: number;
    left: number;
    right: number;
}

interface BoardState {
    board: Board; // The current state of the game board (tiles placed).
    layout: string[][]; // The multiplier layout of the board.
    expandable: boolean; // Whether the current game mode supports board expansion.
    moves: Move[]; // The list of suggested moves from the solver.
    preview: MovePreview | null; // The move currently being hovered over in the WordList.
    zoom: number; // The current zoom level of the board view.
    bands: BandCounters; // Counters for board expansions.
    // Tracks the coordinate system's origin shift after expansions.
    shiftRow: number;
    shiftCol: number;
}

const initialState: BoardState = safeGet<BoardState>("boardState", {
    board: createEmptyBoard(),
    layout: [],
    expandable: true,
    moves: [],
    preview: null,
    zoom: 1,
    bands: { up: 0, down: 0, left: 0, right: 0 },
    // Initialize shift values.
    shiftRow: 0,
    shiftCol: 0,
});

const boardSlice = createSlice({
    name: "board",
    initialState: initialState,
    reducers: {

        setBoard(state, action: PayloadAction<Board>) {
            state.board = action.payload;
        },

        setLayout(
            state,
            action: PayloadAction<{ matrix: string[][]; expandable: boolean }>,
        ) {
            state.layout = action.payload.matrix;
            state.expandable = action.payload.expandable;
        },

        setMoves(state, action: PayloadAction<Move[]>) {
            state.moves = action.payload;
        },

        setPreview(state, action: PayloadAction<MovePreview | null>) {
            state.preview = action.payload;
        },

        setZoom(state, action: PayloadAction<number>) {
            state.zoom = action.payload;
        },

        incrementBand(state, action: PayloadAction<BandDirection>) {
            state.bands[action.payload] += 1;
        },

        resetBands(state) {
            state.bands = { up: 0, down: 0, left: 0, right: 0 };
            // Also reset the shift values to their defaults.
            state.shiftRow = 0;
            state.shiftCol = 0;
        },

        // Updates the board's coordinate system shift.
        setShift(state, action: PayloadAction<{ shiftRow: number; shiftCol: number }>) {
            state.shiftRow = action.payload.shiftRow;
            state.shiftCol = action.payload.shiftCol;
        },

        clearBoard(state) {
            state.board = createEmptyBoard(
                state.board.length,
                state.board[0]?.length ?? 15,
            );
            state.moves = [];
            state.preview = null;
            state.zoom = 1;
        },
    },
});

export const {
    setBoard,
    setLayout,
    setMoves,
    setPreview,
    setZoom,
    incrementBand,
    resetBands,
    clearBoard,
    setShift,
} = boardSlice.actions;

export default boardSlice.reducer;