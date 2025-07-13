import { configureStore } from "@reduxjs/toolkit";
import boardReducer from "@/app/board/boardSlice";
import rackReducer from "@/app/rack/rackSlice";
import settingsReducer from "@/app/settings/settingsSlice";
import { safeSet } from "@/utils/storage";

// Configures the Redux store with reducers for different parts of the application state.
export const store = configureStore({
    reducer: {
        board: boardReducer,
        rack: rackReducer,
        settings: settingsReducer
    }
});

// Subscribes to store updates to persist the entire state to localStorage.
// This ensures the user's session is saved between visits.
store.subscribe(() => {
    const { board, rack, settings } = store.getState();
    safeSet("boardState", board);
    safeSet("rackState", rack);
    safeSet("settingsState", settings);
});

// Exports the type for the entire state tree.
export type RootState = ReturnType<typeof store.getState>;

// Exports the type for the dispatch function.
export type AppDispatch = typeof store.dispatch;