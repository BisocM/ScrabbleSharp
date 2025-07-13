import React from "react";

// Defines the possible directions for board expansion.
export type ExpandDirection = "up" | "down" | "left" | "right";

interface Props {
    direction: ExpandDirection;
    onClick: () => void;
    className?: string;
}

// Maps expansion directions to their corresponding arrow glyphs.
const directionGlyphMap: Record<ExpandDirection, string> = {
    up: "↑",
    down: "↓",
    left: "←",
    right: "→"
};

/**
 * A button used to expand the game board in one of the four cardinal directions.
 */
const ExpandButton: React.FC<Props> = ({ direction, onClick, className = "" }) => (
    <button
        aria-label={`expand ${direction}`}
        onClick={onClick}
        className={`expand-btn ${className}`}
    >
        {directionGlyphMap[direction]}
    </button>
);

export default ExpandButton;