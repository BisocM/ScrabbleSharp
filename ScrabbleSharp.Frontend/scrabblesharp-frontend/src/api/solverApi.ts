import { createGrpcWebTransport } from "@connectrpc/connect-web";
import { createClient, Client } from "@connectrpc/connect";
import {
    Multiplier, Direction,
    SolveRequest, LayoutRequest, ExpandRequest,
    ExpandDelta, LayoutResponse, SolveResponse,
    Move as MoveMessage,
} from "@/api/grpc/ScrabbleSharp_pb";
import { ScrabbleSolver } from "@/api/grpc/ScrabbleSharp_connect";
import { GameModeId } from "@/data/gameModes";

// Configure the gRPC-web transport to the backend server.
const transport = createGrpcWebTransport({
    baseUrl: import.meta.env.VITE_BACKEND_URL ?? "http://localhost:5144",
    useBinaryFormat: true,
});

/**
 * The gRPC client for interacting with the ScrabbleSolver service.
 */
export const client: Client<typeof ScrabbleSolver> =
    createClient(ScrabbleSolver, transport);

// --- INTERFACES ---

/**
 * @interface BandCounters
 * @description Tracks the number of times the board has been expanded in each direction.
 */
export interface BandCounters {
    up: number;
    down: number;
    left: number;
    right: number;
}

/**
 * @interface Move
 * @description Represents a valid move, as processed for the frontend.
 */
export interface Move {
    word: string;
    startRow: number;
    startCol: number;
    horizontal: boolean;
    score: number;
    definition: string;
}

/**
 * @interface LayoutData
 * @description Contains the board layout matrix and its expandability status.
 */
export interface LayoutData {
    matrix: string[][];
    expandable: boolean;
}

/**
 * @interface Slice
 * @description Represents a new slice of the board from an expansion, including its
 * matrix, offsets, and the new total board dimensions.
 */
export interface Slice {
    matrix: string[][];
    offsetRow: number;
    offsetCol: number;
    newShiftRow: number;
    newShiftCol: number;
    totalRows: number;
    totalCols: number;
}

// --- UTILITY FUNCTIONS ---

/**
 * Converts a gRPC Multiplier enum to its string code representation.
 * @param multiplier The Multiplier enum from the protobuf definition.
 * @returns A string code (e.g., "2L", "3W").
 */
const multiplierToCode = (multiplier: Multiplier): string =>
    multiplier === Multiplier.DOUBLE_LETTER ? "2L"
        : multiplier === Multiplier.TRIPLE_LETTER ? "3L"
            : multiplier === Multiplier.QUADRUPLE_LETTER ? "4L"
                : multiplier === Multiplier.DOUBLE_WORD ? "2W"
                    : multiplier === Multiplier.TRIPLE_WORD ? "3W"
                        : multiplier === Multiplier.QUADRUPLE_WORD ? "4W"
                            : "";

/**
 * Transforms a flat list of multipliers from the API into a 2D grid.
 * @param response The layout or delta response from the API.
 * @returns A 2D string array representing the multiplier layout.
 */
export const toMatrix = (
    { rows, cols, multipliers }: LayoutResponse | ExpandDelta,
): string[][] => {
    const grid = Array.from({ length: rows }, () => Array(cols).fill(""));
    multipliers.forEach((multiplier, index) => {
        grid[Math.floor(index / cols)][index % cols] = multiplierToCode(multiplier);
    });
    return grid;
};

// --- API FUNCTIONS ---

/**
 * Fetches the initial board layout for a given game mode.
 * @param mode - The ID of the current game mode.
 * @returns A promise that resolves to the layout data.
 */
export async function getLayoutMatrix(
    mode: GameModeId,
): Promise<LayoutData> {
    const layoutRequest = new LayoutRequest({ rows: 0, cols: 0 });
    const response = await client.getLayout(
        layoutRequest,
        { headers: { "x-mode": mode } },
    );
    return {
        matrix: toMatrix(response),
        expandable: response.expandable,
    };
}

/**
 * Sends the board and rack to the server to solve for the best possible moves.
 * @param boardString - The board state serialized to a string.
 * @param rackString - The player's rack serialized to a string.
 * @param mode - The ID of the current game mode.
 * @param bands - The current expansion counters.
 * @returns A promise that resolves to an array of possible moves.
 */
export async function solveRack(
    boardString: string,
    rackString: string,
    mode: GameModeId,
    bands: BandCounters,
): Promise<Move[]> {
    const solveRequest = new SolveRequest({ board: boardString, rack: rackString });
    const response: SolveResponse = await client.solve(solveRequest, {
        headers: {
            "x-mode": mode,
            "x-up": bands.up.toString(),
            "x-down": bands.down.toString(),
            "x-left": bands.left.toString(),
            "x-right": bands.right.toString(),
        },
    });
    return response.moves.map(
        (moveMessage: MoveMessage): Move => ({ ...moveMessage })
    );
}

export type DirectionType = "up" | "down" | "left" | "right";

/**
 * Requests a new board slice to expand the grid in a specified direction.
 * @param direction - The direction to expand ("up", "down", "left", or "right").
 * @param mode - The ID of the current game mode.
 * @param bands - The current expansion counters.
 * @returns A promise that resolves to the new slice data, including new total dimensions.
 */
export async function expandLayout(
    direction: DirectionType,
    mode: GameModeId,
    bands: BandCounters,
): Promise<Slice> {
    const dirEnum = direction === "up" ? Direction.UP
        : direction === "down" ? Direction.DOWN
            : direction === "left" ? Direction.LEFT
                : Direction.RIGHT;
    const expandRequest = new ExpandRequest({ dir: dirEnum });

    const response: ExpandDelta = await client.expand(expandRequest, {
        headers: {
            "x-mode": mode,
            "x-up": bands.up.toString(),
            "x-down": bands.down.toString(),
            "x-left": bands.left.toString(),
            "x-right": bands.right.toString(),
        },
    });

    return {
        matrix: toMatrix(response),
        offsetRow: response.offsetRow,
        offsetCol: response.offsetCol,
        newShiftRow: response.shiftRow,
        newShiftCol: response.shiftCol,
        totalRows: response.totalRows,
        totalCols: response.totalCols,
    };
}