import React from "react";
import { letterScores } from "@/data/letterScores";

interface Props {
    character: string | null;
}

/**
 * Renders a single tile in the player's rack. It can be an empty slot,
 * a lettered tile, or a blank tile.
 */
const RackTile: React.FC<Props> = ({ character }) => {
    // Render an empty slot if no character is provided.
    if (!character) {
        return <div className="w-10 h-10 m-0.5 bg-black/20 rounded-md shadow-inner" />;
    }

    const isBlank = character === "_";

    return (
        <div
            className="w-10 h-10 m-0.5 bg-gradient-to-br from-amber-200 to-amber-300
                     dark:from-amber-400 dark:to-amber-500 border border-black/20
                     flex items-center justify-center relative rounded-md shadow-lg select-none"
        >
            <span className="font-bold text-xl tracking-wider text-slate-800 dark:text-gray-900">
                {isBlank ? "" : character}
            </span>
            <span className="absolute bottom-1 right-1.5 text-[0.65rem] font-bold text-gray-700 dark:text-gray-900">
                {isBlank ? 0 : letterScores[character]}
            </span>
        </div>
    );
};

export default RackTile;