import React, {useState} from "react";
import type {Move} from "@/api/solverApi";
import type {MovePreview} from "@/app/board/types";

interface Props {
    moves: Move[];
    solveAttempted: boolean;
    onCommitMove: (move: Move) => void;
    onHover: (preview: MovePreview | null) => void;
    onSelectForDefinition: (move: Move) => void;
}

type SortKey = "score" | "length" | "word";
type SortOrder = "asc" | "desc";

/**
 * Displays a sortable list of possible moves found by the solver.
 * Allows users to preview moves on the board by hovering, place them by clicking,
 * and view definitions.
 */
const WordList: React.FC<Props> = ({
                                       moves,
                                       solveAttempted,
                                       onCommitMove,
                                       onHover,
                                       onSelectForDefinition
                                   }) => {
    const [sortKey, setSortKey] = useState<SortKey>("score");
    const [sortOrder, setSortOrder] = useState<SortOrder>("desc");

    // Handles sorting logic when a column header is clicked.
    const handleSort = (newSortKey: SortKey) => {
        if (newSortKey === sortKey) {
            // If clicking the same key, reverse the order.
            setSortOrder(order => (order === "asc" ? "desc" : "asc"));
        } else {
            // If clicking a new key, set it and default the order.
            setSortKey(newSortKey);
            setSortOrder(newSortKey === "word" ? "asc" : "desc");
        }
    };

    // Returns a sort direction indicator (▲ or ▼) for the active sort column.
    const getSortCaret = (key: SortKey) =>
        key === sortKey ? (sortOrder === "asc" ? "▲" : "▼") : " ";

    // Sorts the moves based on the current sort key and order.
    const sortedMoves = [...moves].sort((moveA, moveB) => {
        let difference = 0;
        if (sortKey === "score") {
            difference = moveA.score - moveB.score;
        } else if (sortKey === "length") {
            difference = moveA.word.length - moveB.word.length;
        } else {
            difference = moveA.word.localeCompare(moveB.word);
        }
        return sortOrder === "desc" ? -difference : difference;
    });

    const handleMouseEnter = (move: Move) => onHover({
        word: move.word,
        startRow: move.startRow,
        startCol: move.startCol,
        horizontal: move.horizontal
    });
    const handleMouseLeave = () => onHover(null);

    const emptyListText = solveAttempted
        ? "No valid moves could be found for this position."
        : "Enter tiles and press Solve to see suggestions.";

    return (
        <div className="flex-grow overflow-y-auto">
            <table className="w-full table-fixed text-sm border-separate border-spacing-0">
                <colgroup>
                    <col style={{width: "50%"}}/>
                    <col style={{width: "25%"}}/>
                    <col style={{width: "25%"}}/>
                </colgroup>

                <thead className="sticky top-0 z-10">
                <tr className="bg-slate-100 dark:bg-slate-700">
                    <th className="px-4 py-2 font-semibold cursor-pointer select-none border-b border-slate-200 dark:border-slate-700 text-left"
                        onClick={() => handleSort("word")}>Word <span
                        className="inline-block w-4 text-slate-400">{getSortCaret("word")}</span></th>
                    <th className="px-4 py-2 font-semibold cursor-pointer select-none border-b border-slate-200 dark:border-slate-700 text-right"
                        onClick={() => handleSort("score")}>Pts <span
                        className="inline-block w-4 text-slate-400">{getSortCaret("score")}</span></th>
                    <th className="px-4 py-2 font-semibold cursor-pointer select-none border-b border-slate-200 dark:border-slate-700 text-right"
                        onClick={() => handleSort("length")}>Len <span
                        className="inline-block w-4 text-slate-400">{getSortCaret("length")}</span></th>
                </tr>
                </thead>

                <tbody>
                {sortedMoves.map((move, index) => (
                    <tr key={index} onMouseEnter={() => handleMouseEnter(move)} onMouseLeave={handleMouseLeave}
                        className="group cursor-pointer transition-colors hover:bg-teal-50 dark:hover:bg-teal-800/50">
                        <td className="px-4 py-2 border-b border-slate-100 dark:border-slate-700/50 truncate font-medium"
                            onClick={() => onCommitMove(move)}>{move.word}</td>
                        <td className="px-4 py-2 border-b border-slate-100 dark:border-slate-700/50 text-right"
                            onClick={() => onCommitMove(move)}>{move.score}</td>
                        <td className="px-4 py-2 border-b border-slate-100 dark:border-slate-700/50 flex justify-end items-center"
                            onClick={() => onCommitMove(move)}>
                            <span>{move.word.length}</span>
                            <button onClick={event => {
                                event.stopPropagation();
                                onSelectForDefinition(move);
                            }}
                                    className="ml-2 text-xs opacity-0 group-hover:opacity-60 hover:opacity-100">📖
                            </button>
                        </td>
                    </tr>
                ))}

                {!moves.length && (
                    <tr>
                        <td colSpan={3} className="py-12 px-6 text-center text-gray-500 dark:text-gray-400">
                            {emptyListText}
                        </td>
                    </tr>
                )}
                </tbody>
            </table>
        </div>
    );
};

export default WordList;