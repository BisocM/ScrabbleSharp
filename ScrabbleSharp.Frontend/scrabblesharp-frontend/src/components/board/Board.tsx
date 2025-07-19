import React, {
    useState,
    useRef,
    useEffect,
    KeyboardEvent as ReactKeyboardEvent,
    FormEvent,
} from "react";
import BoardTile from "./BoardTile";
import {TILE_SIZE, TILE_GAP} from "./BoardConstants";
import type {Board as BoardType, MovePreview} from "@/app/board/types";
import {cloneBoard} from "@/utils/boardUtils";
import {useBoardState} from "@/app/board/hooks";
import {usePrevious} from "@/hooks/usePrevious";

interface Props {
    board: BoardType;
    multipliers: string[][];
    onBoardChange: (newBoard: BoardType) => void;
    preview: MovePreview | null;
    trueCenter: { row: number; col: number };
    scale: number;
    onZoom: (delta: number) => void;
}

const Board: React.FC<Props> = ({
                                    board,
                                    multipliers,
                                    onBoardChange,
                                    preview,
                                    trueCenter,
                                    scale,
                                    onZoom,
                                }) => {
    const [selection, setSelection] = useState<{ row: number; col: number } | null>(null);
    const [direction, setDirection] = useState<"h" | "v">("h");
    const [isWaitingForBlankTileLetter, setIsWaitingForBlankTileLetter] = useState(false);
    const inputRef = useRef<HTMLInputElement>(null);

    usePrevious({shiftRow: useBoardState().shiftRow, shiftCol: useBoardState().shiftCol});

    const [panOffset, setPanOffset] = useState({x: 0, y: 0});
    const containerRef = useRef<HTMLDivElement>(null);
    const pointers = useRef(new Map<number, PointerEvent>());
    const panOrigin = useRef({x: 0, y: 0});
    const panBaseOffset = useRef({x: 0, y: 0});
    const lastPinchDistance = useRef(0);

    useEffect(() => {
        const el = containerRef.current;
        if (!el) return;

        const handleWheel = (event: WheelEvent) => {
            event.preventDefault();
            onZoom(event.deltaY < 0 ? 0.1 : -0.1);
        };

        const getPointersArray = () => Array.from(pointers.current.values());

        const getPointersDistance = (p: PointerEvent[]) => {
            return Math.sqrt(Math.pow(p[1].clientX - p[0].clientX, 2) + Math.pow(p[1].clientY - p[0].clientY, 2));
        };

        const getPointersCenter = (p: PointerEvent[]) => {
            return {
                x: (p[0].clientX + p[1].clientX) / 2,
                y: (p[0].clientY + p[1].clientY) / 2,
            };
        };

        const handlePointerDown = (event: PointerEvent) => {
            if (event.pointerType === 'mouse' && event.button === 0) {
                event.preventDefault();
                return;
            }

            el.setPointerCapture(event.pointerId);
            pointers.current.set(event.pointerId, event);

            const currentPointers = getPointersArray();
            if (currentPointers.length === 1 && event.button === 1) { // Middle Mouse Pan Start
                panOrigin.current = {x: event.clientX, y: event.clientY};
                panBaseOffset.current = panOffset;
            } else if (currentPointers.length === 2) { // Two-finger Pan/Pinch Start
                panOrigin.current = getPointersCenter(currentPointers);
                panBaseOffset.current = panOffset;
                lastPinchDistance.current = getPointersDistance(currentPointers);
            }
        };

        const handlePointerMove = (event: PointerEvent) => {
            if (!pointers.current.has(event.pointerId)) return;
            pointers.current.set(event.pointerId, event);

            const currentPointers = getPointersArray();
            if (currentPointers.length === 1 && event.buttons === 4) { // Middle Mouse Pan Move
                const dx = event.clientX - panOrigin.current.x;
                const dy = event.clientY - panOrigin.current.y;
                setPanOffset({x: panBaseOffset.current.x + dx, y: panBaseOffset.current.y + dy});
            } else if (currentPointers.length === 2) { // Two-finger Pan/Pinch Move

                const center = getPointersCenter(currentPointers);
                const dx = center.x - panOrigin.current.x;
                const dy = center.y - panOrigin.current.y;
                setPanOffset({x: panBaseOffset.current.x + dx, y: panBaseOffset.current.y + dy});

                const distance = getPointersDistance(currentPointers);
                if (lastPinchDistance.current > 0) {
                    const scaleDelta = (distance / lastPinchDistance.current - 1) * 0.75;
                    onZoom(scaleDelta);
                }
                lastPinchDistance.current = distance;
            }
        };

        const handlePointerUp = (event: PointerEvent) => {
            pointers.current.delete(event.pointerId);
            if (pointers.current.size < 2) {
                lastPinchDistance.current = 0;
            }
        };

        el.addEventListener("wheel", handleWheel, {passive: false});
        el.addEventListener('pointerdown', handlePointerDown);
        el.addEventListener('pointermove', handlePointerMove);
        el.addEventListener('pointerup', handlePointerUp);
        el.addEventListener('pointercancel', handlePointerUp);

        return () => {
            el.removeEventListener("wheel", handleWheel);
            el.removeEventListener('pointerdown', handlePointerDown);
            el.removeEventListener('pointermove', handlePointerMove);
            el.removeEventListener('pointerup', handlePointerUp);
            el.removeEventListener('pointercancel', handlePointerUp);
        };
    }, [onZoom, panOffset]);

    const placeTile = (row: number, column: number, letter: string, isBlank: boolean) => {
        const newBoard = cloneBoard(board);
        newBoard[row][column] = {letter, isBlank};
        onBoardChange(newBoard);
    };

    const removeTile = (row: number, column: number) => {
        const newBoard = cloneBoard(board);
        newBoard[row][column] = null;
        onBoardChange(newBoard);
    };

    const advanceSelection = (row: number, column: number) => {
        let nextRow = row;
        let nextCol = column;
        direction === "h" ? nextCol++ : nextRow++;

        if (nextRow < board.length && nextCol < (board[0]?.length ?? 0)) {
            setSelection({row: nextRow, col: nextCol});
        } else {
            setSelection(null);
        }
    };

    const handleBoardInput = (event: FormEvent<HTMLInputElement>) => {
        if (!selection) return;

        const {row, col} = selection;
        const value = event.currentTarget.value;
        if (!value) return;

        const character = value.slice(-1);
        const isLetter = /^[A-Za-z]$/.test(character);
        const isBlankChar = /[_? ]/.test(character);

        if (isWaitingForBlankTileLetter) {
            if (isLetter) {
                placeTile(row, col, character.toUpperCase(), true);
                setIsWaitingForBlankTileLetter(false);
                advanceSelection(row, col);
            } else {
                setIsWaitingForBlankTileLetter(false);
            }
        } else if (isLetter) {
            placeTile(row, col, character.toUpperCase(), false);
            advanceSelection(row, col);
        } else if (isBlankChar) {
            setIsWaitingForBlankTileLetter(true);
        }

        event.currentTarget.value = "";
    };

    const handleBoardKeyDown = (event: ReactKeyboardEvent<HTMLInputElement>) => {
        if (!selection) return;

        const {row, col} = selection;
        const key = event.key;

        if (["ArrowUp", "ArrowDown", "ArrowLeft", "ArrowRight", "Backspace", "Delete"].includes(key)) {
            event.preventDefault();
        }

        switch (key) {
            case "ArrowUp":
                if (row > 0) setSelection({row: row - 1, col});
                if (direction !== "v") setDirection("v");
                return;
            case "ArrowDown":
                if (row < board.length - 1) setSelection({row: row + 1, col});
                if (direction !== "v") setDirection("v");
                return;
            case "ArrowLeft":
                if (col > 0) setSelection({row, col: col - 1});
                if (direction !== "h") setDirection("h");
                return;
            case "ArrowRight":
                if (col < (board[0]?.length ?? 0) - 1) setSelection({row, col: col + 1});
                if (direction !== "h") setDirection("h");
                return;
            case "Backspace":
            case "Delete":
                if (isWaitingForBlankTileLetter) {
                    setIsWaitingForBlankTileLetter(false);
                } else if (board[row][col]) {
                    removeTile(row, col);
                } else {
                    const rowStep = direction === "h" ? 0 : -1;
                    const colStep = direction === "h" ? -1 : 0;
                    let previousRow = row + rowStep;
                    let previousCol = col + colStep;

                    if (previousRow >= 0 && previousCol >= 0) {
                        removeTile(previousRow, previousCol);
                        setSelection({row: previousRow, col: previousCol});
                    }
                }
                return;
        }
    };

    const isInPreview = (row: number, column: number): boolean => {
        if (!preview) return false;
        const {startRow, startCol, horizontal, word} = preview;
        return horizontal
            ? row === startRow && column >= startCol && column < startCol + word.length
            : column === startCol && row >= startRow && row < startRow + word.length;
    };

    const getGhostLetter = (row: number, column: number): string | undefined => {
        if (!preview || !isInPreview(row, column)) return undefined;
        const {startRow, startCol, horizontal, word} = preview;
        const index = horizontal ? column - startCol : row - startRow;
        return word[index].toUpperCase();
    };

    const boardPixelWidth = (board[0]?.length ?? 0) * TILE_SIZE + ((board[0]?.length ?? 0) - 1) * TILE_GAP;
    const boardPixelHeight = board.length * TILE_SIZE + (board.length - 1) * TILE_GAP;

    return (
        <div
            ref={containerRef}
            className="relative overflow-hidden rounded-xl border border-yellow-200 dark:border-green-800/30 bg-gradient-to-br from-yellow-50 to-yellow-100 dark:from-green-900 dark:to-green-800 shadow-lg select-none w-full h-[500px] lg:h-[38rem]"
            style={{touchAction: 'none'}}
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
                        style={{height: TILE_SIZE, marginBottom: rowIndex < board.length - 1 ? TILE_GAP : 0}}
                    >
                        {row.map((cell, colIndex) => {
                            const isSelected = selection?.row === rowIndex && selection?.col === colIndex;
                            const showArrow = isSelected && !cell;
                            const arrowIndicator = showArrow ? (direction === "h" ? "→" : "↓") : undefined;
                            return (
                                <BoardTile
                                    key={colIndex}
                                    cell={cell}
                                    multiplier={multipliers?.[rowIndex]?.[colIndex] ?? ""}
                                    isCenter={rowIndex === trueCenter.row && colIndex === trueCenter.col}
                                    isSelected={isSelected}
                                    isHighlighted={isInPreview(rowIndex, colIndex)}
                                    ghostLetter={cell ? undefined : getGhostLetter(rowIndex, colIndex)}
                                    arrowIndicator={arrowIndicator}
                                    onClick={() => {
                                        if (isSelected) {
                                            setDirection(d => (d === "h" ? "v" : "h"));
                                        } else {
                                            setSelection({row: rowIndex, col: colIndex});
                                        }
                                        inputRef.current?.focus();
                                    }}
                                    style={{
                                        width: TILE_SIZE,
                                        height: TILE_SIZE,
                                        marginRight: colIndex < row.length - 1 ? TILE_GAP : 0,
                                    }}
                                />
                            );
                        })}
                    </div>
                ))}
            </div>
            <input
                ref={inputRef}
                type="text"
                onInput={handleBoardInput}
                onKeyDown={handleBoardKeyDown}
                className="absolute w-0 h-0 opacity-0"
                autoComplete="off"
                autoCorrect="off"
                autoCapitalize="off"
                spellCheck="false"
            />
        </div>
    );
};

export default Board;