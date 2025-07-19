import React, {useRef, useState, KeyboardEvent as ReactKeyboardEvent, FormEvent} from "react";
import RackTile from "./RackTile";

interface Props {
    rack: string[];
    onRackChange: (newRack: string[]) => void;
    className?: string;
}

const RACK_CAPACITY = 7;

const Rack: React.FC<Props> = ({rack, onRackChange, className}) => {
    const inputRef = useRef<HTMLInputElement>(null);
    const [typedInSessionCount, setTypedInSessionCount] = useState(0);

    const resetTypingSession = () => setTypedInSessionCount(0);

    const commitRackChange = (nextRack: string[]) => onRackChange(nextRack.slice(0, RACK_CAPACITY));

    const insertWhenSpaceAvailable = (character: string) => {
        const nextRack = [...rack];
        nextRack.splice(typedInSessionCount, 0, character);
        setTypedInSessionCount(count => count + 1);
        commitRackChange(nextRack);
    };

    const queueInsertAndReplace = (character: string) => {
        const nextRack = rack.slice();
        nextRack.pop();
        nextRack.unshift(character);
        commitRackChange(nextRack);
    };

    const handleInput = (event: FormEvent<HTMLInputElement>) => {
        const value = event.currentTarget.value;
        if (!value) return;

        const character = value.slice(-1);
        const isLetter = /[A-Za-z]/.test(character);
        const isBlankChar = /[_? ]/.test(character);

        if (isLetter || isBlankChar) {
            const characterToAdd = isLetter ? character.toUpperCase() : "_";
            if (rack.length < RACK_CAPACITY) {
                insertWhenSpaceAvailable(characterToAdd);
            } else {
                queueInsertAndReplace(characterToAdd);
            }
        }

        event.currentTarget.value = "";
    };

    const handleKeyDown = (event: ReactKeyboardEvent<HTMLInputElement>) => {
        if ((event.key === "Backspace" || event.key === "Delete") && rack.length) {
            event.preventDefault();
            if (typedInSessionCount > 0) {
                const nextRack = [...rack];
                nextRack.splice(typedInSessionCount - 1, 1);
                setTypedInSessionCount(count => count - 1);
                commitRackChange(nextRack);
            } else {
                commitRackChange(rack.slice(0, -1));
            }
        }
    };

    return (
        <div
            className={`p-2 rounded-lg bg-gradient-to-br from-amber-600 to-amber-800 shadow-inner-lg ${className ?? ""}`}
            onClick={() => inputRef.current?.focus()}
        >
            <div className="flex p-1 outline-none rounded-md">
                {Array.from({length: RACK_CAPACITY}).map((_, index) => (
                    <RackTile key={index} character={rack[index] ?? null}/>
                ))}
            </div>
            <input
                ref={inputRef}
                type="text"
                onInput={handleInput}
                onKeyDown={handleKeyDown}
                onBlur={resetTypingSession}
                className="absolute w-0 h-0 opacity-0"
                autoComplete="off"
                autoCorrect="off"
                autoCapitalize="off"
                spellCheck="false"
            />
        </div>
    );
};

export default Rack;