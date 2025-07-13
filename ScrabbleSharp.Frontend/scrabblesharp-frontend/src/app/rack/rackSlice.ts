import { createSlice, PayloadAction } from "@reduxjs/toolkit";
import { safeGet } from "@/utils/storage";

// The rack is represented as an array of single-character strings.
type Rack = string[];

// Defines the shape of the state for the rack slice.
interface RackState { rack: Rack; }

// Safely loads the initial state from localStorage, providing an empty rack as a default.
const initialState: RackState = safeGet<RackState>("rackState", { rack: [] });

// Creates the Redux slice for managing rack state.
const rackSlice = createSlice({
    name: "rack",
    initialState: initialState,
    reducers: {
        // Replaces the entire rack with a new one.
        setRack(state, action: PayloadAction<Rack>) { state.rack = action.payload; },
        // Clears all tiles from the rack.
        clearRack(state) { state.rack = []; }
    }
});

// Export the action creators.
export const { setRack, clearRack } = rackSlice.actions;

// Export the reducer.
export default rackSlice.reducer;