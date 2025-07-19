import {createSlice, PayloadAction} from "@reduxjs/toolkit";
import {safeGet} from "@/utils/storage";
import {GameModeId} from "@/data/gameModes";

// Defines the shape of the application's settings state.
interface SettingsState {
    mode: GameModeId;
    language: "en" | "fr" | "es";
    theme: "light" | "dark";
}

// Safely loads initial settings from localStorage. If not present, it provides defaults.
// The theme default is determined by the user's OS preference.
const initialState: SettingsState = safeGet<SettingsState>("settingsState", {
    mode: "letterleague_classic" as GameModeId,
    language: "en",
    theme: window.matchMedia("(prefers-color-scheme: dark)").matches ? "dark" : "light",
});

// Creates the Redux slice for managing settings state.
const settingsSlice = createSlice({
    name: "settings",
    initialState: initialState,
    reducers: {
        // Sets the game mode.
        setMode(state, action: PayloadAction<GameModeId>) {
            state.mode = action.payload;
        },
        // Sets the application language.
        setLanguage(state, action: PayloadAction<SettingsState["language"]>) {
            state.language = action.payload;
        },
        // Toggles the theme between "light" and "dark".
        toggleTheme(state) {
            state.theme = state.theme === "dark" ? "light" : "dark";
        },
    },
});

// Export the action creators.
export const {setMode, setLanguage, toggleTheme} = settingsSlice.actions;

// Export the reducer.
export default settingsSlice.reducer;