import React from "react";

// Defines the necessary properties of a move for displaying its definition.
interface Move {
    word: string;
    definition: string;
}

interface Props {
    move: Move;
}

/**
 * A simple component that displays the definition of a selected word.
 */
const DefinitionBox: React.FC<Props> = ({ move }) => (
    <div className="p-4 border-t border-slate-200 dark:border-slate-700">
        <h3 className="text-md font-semibold mb-1 text-slate-700 dark:text-slate-200">
            Definition of {move.word}:
        </h3>
        <p className="text-sm text-slate-500 dark:text-slate-400">
            {move.definition || "No definition available."}
        </p>
    </div>
);

export default DefinitionBox;