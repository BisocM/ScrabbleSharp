import React, {
    useState,
    useRef,
    useEffect,
    MouseEvent as ReactMouseEvent,
    KeyboardEvent as ReactKeyboardEvent,
    WheelEvent as ReactWheelEvent
} from "react";
import BoardTile from "./BoardTile";
import { TILE_SIZE, TILE_GAP } from "./BoardConstants";
import type { Board as BoardType, MovePreview } from "@/app/board/types";
import { cloneBoard } from "@/utils/boardUtils";
import { useBoardState } from "@/app/board/hooks";
import { usePrevious } from "@/hooks/usePrevious";

/**
 * @interface Props
 * @description Prop types for the Board component.
 */
interface Props {
    /** The 2D array representing the current state of the board. */
    board: BoardType;
    /** A 2D array with string codes for cell multipliers (e.g., "2L", "3W"). */
    multipliers: string[][];
    /** Callback function triggered when the board's state is changed by the user. */
    onBoardChange: (newBoard: BoardType) => void;
    /** Data for previewing a potential move on the board. */
    preview: MovePreview | null;
    /** The coordinates of the board's true center, accounting for expansions. */
    trueCenter: { row: number, col: number };
    /** The current zoom scale factor for the board. */
    scale: number;
    /** Callback function to handle zoom events. */
    onZoom: (delta: number) => void;
}

/**
 * @component Board
 * @description An interactive, pannable, and zoomable Scrabble board. It handles
 * all user interactions directly on the grid, including typing letters, placing blank
 * tiles, deleting tiles, and navigating the selection.
 */
const Board: React.FC<Props> = ({
                                    board,
                                    multipliers,
                                    onBoardChange,
                                    preview,
                                    trueCenter,
                                    scale,
                                    onZoom,
                                }) => {
    // --- STATE MANAGEMENT ---

    // The currently selected cell for input.
    const [selection, setSelection] = useState<{ row: number; col: number } | null>(null);
    // The current input direction, horizontal ('h') or vertical ('v').
    const [direction, setDirection] = useState<"h" | "v">("h");
    // A flag to indicate the app is waiting for the user to specify a letter for a blank tile.
    const [isWaitingForBlankTileLetter, setIsWaitingForBlankTileLetter] = useState(false);

    // Board shift state from Redux, used to adjust panning.
    const { shiftRow, shiftCol } = useBoardState();
    const prevShift = usePrevious({ shiftRow, shiftCol });

    // Panning and dragging state.
    const [panOffset, setPanOffset] = useState({ x: 0, y: 0 });
    const isDragging = useRef(false);
    const dragOrigin = useRef({ x: 0, y: 0 });
    const dragBaseOffset = useRef({ x: 0, y: 0 });

    const rowCount = board.length;
    const columnCount = board[0]?.length ?? 0;


    /**
     * This effect adjusts the pan offset when the board expands. It ensures that the
     * visible area of the board remains centered after new rows or columns are added.
     */
    useEffect(() => {
        const el = containerRef.current;
        if (!el) return;
        // Non-passive wheel listener so preventDefault() works
        el.addEventListener("wheel", handleWheel, { passive: false });
        return () => el.removeEventListener("wheel", handleWheel);
    }, []);

    // --- BOARD MODIFICATION LOGIC ---

    /**
     * Places a new tile onto the board.
     * @param row - The row index.
     * @param column - The column index.
     * @param letter - The letter for the tile.
     * @param isBlank - Whether the tile is a blank.
     */
    const placeTile = (row: number, column: number, letter: string, isBlank: boolean) => {
        const newBoard = cloneBoard(board);
        newBoard[row][column] = { letter, isBlank };
        onBoardChange(newBoard);
    };

    /**
     * Removes a tile from the board.
     * @param row - The row index of the tile to remove.
     * @param column - The column index of the tile to remove.
     */
    const removeTile = (row: number, column: number) => {
        const newBoard = cloneBoard(board);
        newBoard[row][column] = null;
        onBoardChange(newBoard);
    };

    /**
     * Moves the selection to the next cell based on the current direction.
     * @param row - The current row index.
     * @param column - The current column index.
     */
    const advanceSelection = (row: number, column: number) => {
        let nextRow = row;
        let nextCol = column;
        direction === "h" ? nextCol++ : nextRow++;

        if (nextRow < rowCount && nextCol < columnCount) {
            setSelection({ row: nextRow, col: nextCol });
            document.getElementById(`cell-${nextRow}-${nextCol}`)?.focus();
        } else {
            setSelection(null); // Deselect if at the edge.
        }
    };

    // --- EVENT HANDLERS ---

    /**
     * Handles all keyboard inputs on a board tile for placing, deleting,
     * and navigating tiles.
     */
    const handleKeyDown = (event: ReactKeyboardEvent<HTMLDivElement>, row: number, column: number) => {
        if (!selection || selection.row !== row || selection.col !== column) {
            setSelection({ row, col: column });
        }

        const key = event.key;
        const isLetter = /^[A-Za-z]$/.test(key);

        // Prevent default browser behavior for relevant keys.
        if (["ArrowUp", "ArrowDown", "ArrowLeft", "ArrowRight", "Backspace", "Delete", " "].includes(key) || isLetter || ["_", "?"].includes(key)) {
            event.preventDefault();
        }

        // State for handling blank tile letter input.
        if (isWaitingForBlankTileLetter) {
            if (isLetter) {
                placeTile(row, column, key.toUpperCase(), true);
                setIsWaitingForBlankTileLetter(false);
                advanceSelection(row, column);
            } else if (["Backspace", "Delete", "Escape"].includes(key)) {
                setIsWaitingForBlankTileLetter(false);
            }
            return;
        }

        // Handle standard letter input.
        if (isLetter) {
            placeTile(row, column, key.toUpperCase(), false);
            advanceSelection(row, column);
            return;
        }

        // Handle blank tile placement initiation.
        if (key === " " || key === "_" || key === "?") {
            setIsWaitingForBlankTileLetter(true);
            return;
        }

        // Handle backspace and delete.
        if (key === "Backspace" || key === "Delete") {
            if (board[row][column]) {
                removeTile(row, column);
            } else {
                // If the current cell is empty, delete the previous tile in the sequence.
                const rowStep = direction === "h" ? 0 : -1;
                const colStep = direction === "h" ? -1 : 0;
                let previousRow = row + rowStep;
                let previousCol = column + colStep;

                if (previousRow >= 0 && previousCol >= 0) {
                    removeTile(previousRow, previousCol);
                    setSelection({ row: previousRow, col: previousCol });
                    document.getElementById(`cell-${previousRow}-${previousCol}`)?.focus();
                }
            }
            return;
        }

        // Handle arrow keys to change input direction.
        if (key === "ArrowLeft" || key === "ArrowRight") {
            if (direction !== "h") setDirection("h");
        }
        if (key === "ArrowUp" || key === "ArrowDown") {
            if (direction !== "v") setDirection("v");
        }
    };

    /** Handles mouse wheel events for zooming. */
    const containerRef = useRef<HTMLDivElement>(null);
    const handleWheel = (event: WheelEvent) => {
        event.preventDefault();
        onZoom(event.deltaY < 0 ? 0.1 : -0.1);
    };

    /** Initiates panning on middle mouse button down. */
    const handleMouseDown = (event: ReactMouseEvent) => {
        if (event.button !== 1) return; // Only pan with middle mouse button.
        event.preventDefault();
        isDragging.current = true;
        dragOrigin.current = { x: event.clientX, y: event.clientY };
        dragBaseOffset.current = panOffset;
        window.addEventListener("mousemove", handleMouseMove);
        window.addEventListener("mouseup", handleMouseUp);
    };

    /** Updates pan offset during dragging. */
    const handleMouseMove = (event: MouseEvent) => {
        if (!isDragging.current) return;
        const dx = event.clientX - dragOrigin.current.x;
        const dy = event.clientY - dragOrigin.current.y;
        setPanOffset({
            x: dragBaseOffset.current.x + dx,
            y: dragBaseOffset.current.y + dy,
        });
    };

    /** Ends the panning action on mouse up. */
    const handleMouseUp = () => {
        isDragging.current = false;
        window.removeEventListener("mousemove", handleMouseMove);
        window.removeEventListener("mouseup", handleMouseUp);
    };


    // --- PREVIEW AND GHOST TILE LOGIC ---

    /**
     * Checks if a given cell is part of the current move preview.
     * @returns True if the cell is part of the preview, otherwise false.
     */
    const isInPreview = (row: number, column: number): boolean => {
        if (!preview) return false;
        const { startRow, startCol, horizontal, word } = preview;
        return horizontal
            ? row === startRow && column >= startCol && column < startCol + word.length
            : column === startCol && row >= startRow && row < startRow + word.length;
    };

    /**
     * Gets the letter to display as a "ghost" tile from the move preview.
     * @returns The ghost letter or undefined if the cell is not part of the preview.
     */
    const getGhostLetter = (row: number, column: number): string | undefined => {
        if (!preview || !isInPreview(row, column)) return undefined;
        const { startRow, startCol, horizontal, word } = preview;
        const index = horizontal ? column - startCol : row - startRow;
        return word[index].toUpperCase();
    };

    const boardPixelWidth = columnCount * TILE_SIZE + (columnCount - 1) * TILE_GAP;
    const boardPixelHeight = rowCount * TILE_SIZE + (rowCount - 1) * TILE_GAP;

    // --- RENDER ---

    return (
        <div
            ref={containerRef}
            onMouseDown={handleMouseDown}
            className="relative overflow-hidden rounded-xl border border-yellow-200 dark:border-green-800/30 bg-gradient-to-br from-yellow-50 to-yellow-100 dark:from-green-900 dark:to-green-800 shadow-lg select-none w-full h-[38rem] cursor-grab active:cursor-grabbing"
        >
            <div
                className="absolute top-0 left-0 origin-top-left"
                style={{
                    transform: `translate(${panOffset.x}px, ${panOffset.y}px) scale(${scale})`,
                    width: boardPixelWidth,
                    height: boardPixelHeight,
                }}
            >
                {board.map((row, rowIndex) => (
                    <div
                        key={rowIndex}
                        className="flex"
                        style={{ height: TILE_SIZE, marginBottom: rowIndex < rowCount - 1 ? TILE_GAP : 0 }}
                    >
                        {row.map((cell, colIndex) => {
                            const isSelected = selection?.row === rowIndex && selection?.col === colIndex;
                            const showArrow = isSelected && !cell;
                            const arrowIndicator = showArrow ? (direction === "h" ? "→" : "↓") : undefined;

                            return (
                                <BoardTile
                                    key={colIndex}
                                    id={`cell-${rowIndex}-${colIndex}`}
                                    cell={cell}
                                    multiplier={multipliers?.[rowIndex]?.[colIndex] ?? ""}
                                    isCenter={rowIndex === trueCenter.row && colIndex === trueCenter.col}
                                    isSelected={!!isSelected}
                                    isHighlighted={isInPreview(rowIndex, colIndex)}
                                    ghostLetter={cell ? undefined : getGhostLetter(rowIndex, colIndex)}
                                    arrowIndicator={arrowIndicator}
                                    onClick={() => {
                                        if (isSelected) {
                                            setDirection(d => (d === "h" ? "v" : "h"));
                                        } else {
                                            setSelection({ row: rowIndex, col: colIndex });
                                        }
                                        document.getElementById(`cell-${rowIndex}-${colIndex}`)?.focus();
                                    }}
                                    onKeyDown={(event) => handleKeyDown(event, rowIndex, colIndex)}
                                    style={{
                                        width: TILE_SIZE,
                                        height: TILE_SIZE,
                                        marginRight: colIndex < columnCount - 1 ? TILE_GAP : 0,
                                    }}
                                />
                            );
                        })}
                    </div>
                ))}
            </div>
        </div>
    );
};

export default Board;