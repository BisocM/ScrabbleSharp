import React from "react";
import {letterScores} from "@/data/letterScores";
import clsx from "clsx";

interface Cell {
    letter: string;
    isBlank: boolean;
}

interface Props {
    cell: Cell | null;
    multiplier: string;
    isCenter: boolean;
    isSelected: boolean;
    isHighlighted: boolean;
    arrowIndicator?: string;
    ghostLetter?: string;
    onClick: () => void;
    style?: React.CSSProperties;
}

const multiplierColorMap: Record<string, string> = {
    "": "bg-[--color-surface] dark:bg-green-400/10",
    "2L": "bg-[--color-bonus2L] text-sky-900 dark:bg-sky-600/80 dark:text-white",
    "3L": "bg-[--color-bonus3L] text-yellow-900 dark:bg-blue-800 dark:text-white",
    "4L": "bg-[--color-bonus3L] text-yellow-900 dark:bg-blue-900 dark:text-white",
    "2W": "bg-[--color-bonus2W] text-green-900 dark:bg-rose-600/80 dark:text-white",
    "3W": "bg-[--color-bonus3W] text-red-900 dark:bg-red-800 dark:text-white",
    "4W": "bg-[--color-bonus3W] text-red-900 dark:bg-red-900 dark:text-white",
};

const computeDisplayedScore = (letter: string, multiplier: string): number => {
    const baseScore = letterScores[letter.toUpperCase()] ?? 0;
    const factor = /^[234]L$/.test(multiplier) ? parseInt(multiplier[0], 10) : 1;
    return baseScore * factor;
};

const BoardTile: React.FC<Props> = ({
                                        cell,
                                        multiplier,
                                        isCenter,
                                        isSelected,
                                        isHighlighted,
                                        arrowIndicator,
                                        ghostLetter,
                                        onClick,
                                        style,
                                    }) => {
    const baseClasses =
        "text-xs relative outline-none flex items-center justify-center " +
        "border-b border-r border-black/10 dark:border-white/10";

    const ringClasses = isSelected
        ? "relative z-20 ring-4 ring-offset-2 ring-offset-[--color-surface] dark:ring-offset-green-900 ring-cyan-500"
        : isHighlighted
            ? "relative z-20 ring-2 ring-teal-400"
            : "";

    if (arrowIndicator) {
        return (
            <div
                onClick={onClick}
                className={clsx(baseClasses, multiplierColorMap[multiplier], ringClasses, "transition-shadow")}
                style={style}
            >
                <span className="text-3xl font-bold opacity-90">{arrowIndicator}</span>
            </div>
        );
    }

    if (!cell) {
        return (
            <div
                onClick={onClick}
                className={clsx(baseClasses, multiplierColorMap[multiplier], ringClasses, "transition-shadow")}
                style={style}
            >
                {ghostLetter ? (

                    <span className="font-bold text-slate-500 dark:text-slate-300 text-lg opacity-80">
                        {ghostLetter}
                    </span>
                ) : (

                    <span className="text-sm font-bold opacity-90">
                {isCenter ? "★" : multiplier}
            </span>
                )
                }
            </div>
        )
            ;
    }

    const displayedScore = cell.isBlank
        ? 0
        : computeDisplayedScore(cell.letter, multiplier);

    return (
        <div
            onClick={onClick}
            className={clsx(
                baseClasses,
                "bg-gradient-to-br from-amber-200 to-amber-300 dark:from-amber-400 dark:to-amber-500",
                "font-bold text-slate-800 dark:text-gray-900 shadow-md",
                "hover:-translate-y-0.5 active:translate-y-0 transition-transform duration-75 rounded-md",
                ringClasses
            )}
            style={style}
        >
            <span className="text-lg tracking-wider">
                {cell.letter.toUpperCase()}
            </span>
            <span className="absolute bottom-0.5 right-1 text-[0.6rem] font-bold text-gray-700 dark:text-gray-900">
                {displayedScore}
            </span>
        </div>
    );
};

export default BoardTile;