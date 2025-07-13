import React from "react";
import Modal from "@/components/ui/Modal";
import Button from "@/components/ui/Button";

import { useSettings, useSettingsDispatch } from "@/app/settings/hooks";
import { setMode, setLanguage } from "@/app/settings/settingsSlice";
import { GAME_MODES, GameModeId } from "@/data/gameModes";

interface Props {
    show: boolean;
    onClose: () => void;
}

/**
 * A modal panel for changing application settings, such as game mode and language.
 */
const SettingsPanel: React.FC<Props> = ({ show, onClose }) => {
    const { mode, language } = useSettings();
    const dispatch = useSettingsDispatch();

    const gameModeIds = Object.keys(GAME_MODES) as GameModeId[];

    /**
     * A helper function to render a row of radio buttons for a setting.
     * @param name The name attribute for the radio inputs.
     * @param options The array of available options.
     * @param value The currently selected value.
     * @param onChange The callback to fire when a new option is selected.
     * @param labelMap An optional map from option values to display labels.
     * @param isDisabled An optional function to determine if an option should be disabled.
     */
    const radioRow = <T extends string>(
        name: string,
        options: readonly T[],
        value: T,
        onChange: (selectedValue: T) => void,
        labelMap?: Record<T, string>,
        isDisabled?: (optionValue: T) => boolean,
    ) => (
        <div className="space-y-2">
            {options.map(option => (
                <label
                    key={option}
                    className={`flex items-center p-3 rounded-lg transition-colors ${
                        value === option
                            ? "bg-blue-50 dark:bg-blue-900/50"
                            : "hover:bg-slate-100 dark:hover:bg-slate-700/50"
                    } ${isDisabled?.(option) ? "opacity-50 cursor-not-allowed" : "cursor-pointer"}`}
                >
                    <input
                        type="radio"
                        name={name}
                        value={option}
                        disabled={isDisabled?.(option)}
                        checked={value === option}
                        onChange={() => onChange(option)}
                        className="w-4 h-4 text-blue-600 bg-gray-100 border-gray-300 focus:ring-blue-500
                                 dark:focus:ring-blue-600 dark:ring-offset-gray-800 focus:ring-2
                                 dark:bg-gray-700 dark:border-gray-600"
                    />
                    <span className="ml-3 text-sm font-medium">
                        {labelMap?.[option] ?? option}
                    </span>
                </label>
            ))}
        </div>
    );


    return (
        <Modal show={show} title="Settings" onClose={onClose} width="max-w-md">
            <div className="space-y-6">
                {/* Game Mode Selection */}
                <section>
                    <p className="font-semibold mb-2 text-slate-600 dark:text-slate-300">
                        Game Mode
                    </p>
                    {radioRow(
                        "mode",
                        gameModeIds,
                        mode,
                        (selectedValue) => dispatch(setMode(selectedValue)),
                        Object.fromEntries(
                            gameModeIds.map(id => [id, GAME_MODES[id].label]),
                        ) as Record<GameModeId, string>,
                    )}
                </section>

                {/* Language Selection */}
                <section>
                    <p className="font-semibold mb-2 text-slate-600 dark:text-slate-300">
                        Language
                    </p>
                    {radioRow(
                        "language",
                        ["en", "fr", "es"] as const,
                        language,
                        (selectedValue) => dispatch(setLanguage(selectedValue)),
                        { en: "English", fr: "Français (soon)", es: "Español (soon)" },
                        (optionValue) => optionValue !== "en", // Disable non-English options for now
                    )}
                </section>

                <div className="text-right">
                    <Button variant="secondary" onClick={onClose}>
                        Close
                    </Button>
                </div>
            </div>
        </Modal>
    );
};

export default SettingsPanel;