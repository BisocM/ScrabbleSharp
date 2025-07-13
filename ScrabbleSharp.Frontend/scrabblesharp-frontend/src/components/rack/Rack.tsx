import React, { useRef, useState, KeyboardEvent as ReactKeyboardEvent } from "react";
import RackTile from "./RackTile";

interface Props {
    rack: string[];
    onRackChange: (newRack: string[]) => void;
    className?: string;
}

// The maximum number of tiles allowed in the rack.
const RACK_CAPACITY = 7;

/**
 * A component that displays the player's tile rack and handles keyboard input
 * for adding, removing, and changing tiles.
 */
const Rack: React.FC<Props> = ({ rack, onRackChange, className }) => {
    const rackContainerRef = useRef<HTMLDivElement>(null);

    // Tracks how many tiles have been typed in the current focus session.
    // This helps manage where backspace removes from.
    const [typedInSessionCount, setTypedInSessionCount] = useState(0);

    // Resets the session typing counter when the component loses focus.
    const resetTypingSession = () => setTypedInSessionCount(0);

    // Commits the next state of the rack, ensuring it doesn't exceed capacity.
    const commitRackChange = (nextRack: string[]) => onRackChange(nextRack.slice(0, RACK_CAPACITY));

    // Inserts a character if there is space in the rack.
    const insertWhenSpaceAvailable = (character: string) => {
        const nextRack = [...rack];
        nextRack.splice(typedInSessionCount, 0, character);
        setTypedInSessionCount(count => count + 1);
        commitRackChange(nextRack);
    };

    // Replaces the oldest tile with a new one when the rack is full.
    const queueInsertAndReplace = (character: string) => {
        const nextRack = rack.slice();
        nextRack.pop();
        nextRack.unshift(character);
        commitRackChange(nextRack);
    };

    const keyHandler = (event: ReactKeyboardEvent<HTMLDivElement>) => {
        const key = event.key;

        // Handle backspace/delete.
        if ((key === "Backspace" || key === "Delete") && rack.length) {
            event.preventDefault();
            if (typedInSessionCount > 0) {
                // If typed this session, remove the last typed character.
                const nextRack = [...rack];
                nextRack.splice(typedInSessionCount - 1, 1);
                setTypedInSessionCount(count => count - 1);
                commitRackChange(nextRack);
            } else {
                // Otherwise, remove the last tile on the rack.
                commitRackChange(rack.slice(0, -1));
            }
            return;
        }

        const isSingleChar = key.length === 1;
        const isLetter = isSingleChar && /[A-Za-z]/.test(key);
        const isBlankChar = isSingleChar && (key === "_" || key === "?" || key === " ");

        if (!(isLetter || isBlankChar)) return;

        event.preventDefault();
        const characterToAdd = isLetter ? key.toUpperCase() : "_";

        if (rack.length < RACK_CAPACITY) {
            insertWhenSpaceAvailable(characterToAdd);
        } else {
            queueInsertAndReplace(characterToAdd);
        }
    };

    return (
        <div
            className={`p-2 rounded-lg bg-gradient-to-br from-amber-600 to-amber-800 shadow-inner-lg ${className ?? ""}`}
        >
            <div
                ref={rackContainerRef}
                tabIndex={0}
                onKeyDown={keyHandler}
                onBlur={resetTypingSession}
                onClick={() => rackContainerRef.current?.focus()}
                className="flex p-1 outline-none focus:ring-2 ring-offset-4 ring-offset-amber-700 focus:ring-blue-400 rounded-md"
            >
                {Array.from({ length: RACK_CAPACITY }).map((_, index) => (
                    <RackTile key={index} character={rack[index] ?? null} />
                ))}
            </div>
        </div>
    );
};

export default Rack;