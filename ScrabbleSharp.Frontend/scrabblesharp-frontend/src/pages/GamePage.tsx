import React, { useEffect, useState, useCallback } from "react";
import toast from "react-hot-toast";

import Board from "@/components/board/Board";
import Rack from "@/components/rack/Rack";
import WordList from "@/components/wordlist/WordList";
import DefinitionBox from "@/components/wordlist/DefinitionBox";
import SettingsPanel from "@/components/settings/SettingsPanel";
import Toggle from "@/components/ui/Toggle";
import Button from "@/components/ui/Button";
import ExpandButton from "@/components/board/ExpandButton";
import HowToUse from "@/components/info/HowToUse";
import Logo from "@/components/ui/Logo"; // Import the new component

import { useBoardState, useBoardDispatch } from "@/app/board/hooks";
import { useRack, useRackDispatch } from "@/app/rack/hooks";
import { useSettings } from "@/app/settings/hooks";

import {
    setBoard,
    setLayout,
    setMoves,
    setPreview,
    createEmptyBoard,
    incrementBand,
    resetBands,
    setShift,
} from "@/app/board/boardSlice";
import { setRack } from "@/app/rack/rackSlice";

import { boardToString } from "@/utils/boardUtils";
import { rackToString } from "@/utils/rackUtils";

import {
    getLayoutMatrix,
    solveRack,
    expandLayout,
    DirectionType,
    Move as ApiMove,
} from "@/api/solverApi";
import { ConnectError, Code } from "@connectrpc/connect";

import type { Board as BoardType } from "@/app/board/types";
import { getModeInfo } from "@/data/gameModes";

const GamePage: React.FC = () => {

    const {
        board,
        layout,
        expandable,
        moves,
        preview,
        bands,
        shiftRow,
        shiftCol,
    } = useBoardState();
    const rack = useRack();
    const { mode } = useSettings();

    const boardDispatch = useBoardDispatch();
    const rackDispatch = useRackDispatch();

    const [definitionMove, setDefinitionMove] = useState<ApiMove | null>(null);
    const [showSettings, setShowSettings] = useState(false);
    const [isFlashing, setIsFlashing] = useState(false);
    const [zoom, setZoom] = useState(1);
    const [solveAttempted, setSolveAttempted] = useState(false);
    const [isClearArmed, setIsClearArmed] = useState(false);
    const [isSolving, setIsSolving] = useState(false);

    const clampZoom = (z: number) => Math.min(2, Math.max(0.5, z));

    const triggerFlash = () => {
        setIsFlashing(true);
        setTimeout(() => setIsFlashing(false), 600);
    };

    const handleRpcError = (error: unknown) => {
        if (error instanceof ConnectError && error.code === Code.Unavailable) {
            toast.error("Slow down — too many requests.");
            triggerFlash();
        } else {
            toast.error(String(error));
        }
    };

    const resetBoardAndLayout = useCallback(async () => {
        try {
            const { matrix, expandable } = await getLayoutMatrix(mode);
            boardDispatch(setLayout({ matrix, expandable }));
            boardDispatch(setBoard(createEmptyBoard(matrix.length, matrix[0].length)));
            boardDispatch(resetBands());
        } catch (e) {
            handleRpcError(e);
        }
    }, [mode, boardDispatch]);

    useEffect(() => {
        resetBoardAndLayout();
    }, [resetBoardAndLayout]);

    const invalidateMoves = () => {
        setSolveAttempted(false);
        if (moves.length) boardDispatch(setMoves([]));
        boardDispatch(setPreview(null));
        setDefinitionMove(null);
    };

    const handleBoardChange = (newBoard: BoardType) => {
        boardDispatch(setBoard(newBoard));
        invalidateMoves();
    };

    const handleRackChange = (newRack: string[]) => {
        rackDispatch(setRack(newRack));
        invalidateMoves();
    };

    const commitMove = (move: ApiMove) => {
        const newBoard = board.map((row) => row.slice()) as BoardType;
        const newRack = [...rack];

        for (let i = 0; i < move.word.length; i++) {
            const row = move.startRow + (move.horizontal ? 0 : i);
            const col = move.startCol + (move.horizontal ? i : 0);

            if (newBoard[row][col]) continue;

            const letter = move.word[i].toUpperCase();
            let rackIdx = newRack.indexOf(letter);
            let isBlank = false;

            if (rackIdx === -1) {
                rackIdx = newRack.indexOf("_");
                isBlank = rackIdx !== -1;
            }
            if (rackIdx === -1) continue;

            newRack.splice(rackIdx, 1);
            newBoard[row][col] = { letter, isBlank };
        }

        boardDispatch(setBoard(newBoard));
        rackDispatch(setRack(newRack));
        invalidateMoves();
    };

    const handleExpand = async (direction: DirectionType) => {
        if (!expandable) return;
        try {
            const {
                matrix: sliceMatrix,
                offsetRow,
                offsetCol,
                newShiftRow,
                newShiftCol,
                totalRows,
                totalCols,
            } = await expandLayout(direction, mode, bands);

            const newBoard = createEmptyBoard(totalRows, totalCols);
            const newLayout = Array.from({ length: totalRows }, () =>
                Array.from({ length: totalCols }, () => ""),
            );

            const rowDelta = newShiftRow - shiftRow;
            const colDelta = newShiftCol - shiftCol;

            for (let r = 0; r < board.length; r++) {
                for (let c = 0; c < board[r].length; c++) {
                    const nr = r + rowDelta;
                    const nc = c + colDelta;
                    if (board[r][c]) newBoard[nr][nc] = board[r][c];
                    if (layout[r][c]) newLayout[nr][nc] = layout[r][c];
                }
            }

            for (let r = 0; r < sliceMatrix.length; r++) {
                for (let c = 0; c < sliceMatrix[r].length; c++) {
                    newLayout[offsetRow + r][offsetCol + c] = sliceMatrix[r][c];
                }
            }

            boardDispatch(setBoard(newBoard));
            boardDispatch(setLayout({ matrix: newLayout, expandable }));
            boardDispatch(incrementBand(direction));
            boardDispatch(setShift({ shiftRow: newShiftRow, shiftCol: newShiftCol }));
        } catch (e) {
            handleRpcError(e);
        }
    };

    const handleSolve = async () => {
        if (isSolving) return;

        if (rack.length === 0) {
            toast.error("Your rack is empty.");
            return;
        }

        setIsSolving(true);
        setSolveAttempted(true);

        try {
            const solvedMoves = await solveRack(
                boardToString(board),
                rackToString(rack),
                mode,
                bands,
            );
            boardDispatch(setMoves(solvedMoves));
            boardDispatch(setPreview(null));
            setDefinitionMove(null);
        } catch (e) {
            handleRpcError(e);
        } finally {
            setIsSolving(false);
        }
    };

    const handleClearGame = async () => {

        if (!isClearArmed) {
            setIsClearArmed(true);
            toast("Click Clear once more to confirm.", { icon: "!" });
            setTimeout(() => setIsClearArmed(false), 3000); // Reset after 3 seconds.
            return;
        }

        await resetBoardAndLayout();
        rackDispatch(setRack([]));
        invalidateMoves();
        setIsClearArmed(false);
        toast.success("Board and rack cleared.");
    };

    const handleZoomIn = () => setZoom((z) => clampZoom(z + 0.1));
    const handleZoomOut = () => setZoom((z) => clampZoom(z - 0.1));
    const handleWheelZoom = (delta: number) => setZoom((z) => clampZoom(z + delta));

    const { initialRows, initialCols } = getModeInfo(mode);
    const trueCenter = {
        row: Math.floor(initialRows / 2) + shiftRow,
        col: Math.floor(initialCols / 2) + shiftCol,
    };

    return (
        <>
            {isFlashing && (
                <div className="fixed inset-0 z-[9999] bg-red-500/30 pointer-events-none animate-pulse" />
            )}

            <div className="min-h-screen w-full flex flex-col p-4 gap-4">
                <header className="w-full flex-shrink-0">
                    <div className="max-w-7xl mx-auto flex items-center justify-between p-2 bg-white/50 dark:bg-slate-800/50 rounded-lg shadow-md backdrop-blur-sm">
                        <Logo />
                        <div className="flex items-center gap-2">
                            <Button variant="secondary" onClick={handleClearGame}>
                                {isClearArmed ? "Confirm?" : "Clear"}
                            </Button>
                            <Toggle />
                            <Button
                                variant="secondary"
                                onClick={() => setShowSettings(true)}
                            >
                                Settings
                            </Button>
                        </div>
                    </div>
                </header>

                <main className="flex-grow w-full flex flex-col lg:flex-row items-start justify-center gap-8">

                    {/* Left Column: WordList and HowToUse */}
                    <div className="w-full lg:w-80 flex flex-col order-2 lg:order-1 gap-6">
                        <div className="h-[38rem] bg-white dark:bg-slate-800 rounded-lg shadow-lg flex flex-col">
                            <div className="p-4 border-b border-slate-200 dark:border-slate-700 shrink-0">
                                <h2 className="text-lg font-bold">Possible Moves</h2>
                            </div>
                            <WordList
                                moves={moves}
                                solveAttempted={solveAttempted}
                                onCommitMove={commitMove}
                                onHover={(previewData) =>
                                    boardDispatch(setPreview(previewData))
                                }
                                onSelectForDefinition={setDefinitionMove}
                            />
                            {definitionMove && <DefinitionBox move={definitionMove} />}
                        </div>
                        <HowToUse />
                    </div>

                    {/* Right Column: Board, Rack, and Controls */}
                    <div className="w-full lg:w-[44rem] flex flex-col items-center justify-start gap-6 order-1 lg:order-2">
                        <div className="relative w-full">
                            <div className="absolute right-1 top-1 flex flex-col gap-1 z-20">
                                <Button
                                    variant="secondary"
                                    compact
                                    className="!w-7 !h-7"
                                    onClick={handleZoomIn}
                                >
                                    +
                                </Button>
                                <Button
                                    variant="secondary"
                                    compact
                                    className="!w-7 !h-7"
                                    onClick={handleZoomOut}
                                >
                                    -
                                </Button>
                            </div>

                            <Board
                                board={board}
                                multipliers={layout}
                                onBoardChange={handleBoardChange}
                                preview={preview}
                                trueCenter={trueCenter}
                                scale={zoom}
                                onZoom={handleWheelZoom}
                            />

                            {expandable && (
                                <>
                                    <ExpandButton
                                        direction="up"
                                        onClick={() => handleExpand("up")}
                                        className="top-0 left-1/2 -translate-x-1/2 -translate-y-1/2"
                                    />
                                    <ExpandButton
                                        direction="down"
                                        onClick={() => handleExpand("down")}
                                        className="bottom-0 left-1/2 -translate-x-1/2 translate-y-1/2"
                                    />
                                    <ExpandButton
                                        direction="left"
                                        onClick={() => handleExpand("left")}
                                        className="left-0 top-1/2 -translate-y-1/2 -translate-x-1/2"
                                    />
                                    <ExpandButton
                                        direction="right"
                                        onClick={() => handleExpand("right")}
                                        className="right-0 top-1/2 -translate-y-1/2 translate-x-1/2"
                                    />
                                </>
                            )}
                        </div>

                        <div className="flex flex-col items-center gap-4">
                            <Rack rack={rack} onRackChange={handleRackChange} />
                            <Button
                                variant="primary"
                                className="w-48"
                                onClick={handleSolve}
                                disabled={isSolving}
                            >
                                {isSolving ? "Solving..." : "Solve"}
                            </Button>
                        </div>
                    </div>
                </main>
            </div>

            <SettingsPanel
                show={showSettings}
                onClose={() => setShowSettings(false)}
            />
        </>
    );
};

export default GamePage;