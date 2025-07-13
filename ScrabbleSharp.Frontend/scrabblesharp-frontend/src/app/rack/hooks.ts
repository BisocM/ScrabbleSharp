import { useSelector, useDispatch } from "react-redux";
import type { RootState, AppDispatch } from "@/app/store";

/**
 * A custom hook to access the player's rack from the Redux store.
 * @returns The current rack as an array of strings.
 */
export const useRack = () => useSelector((state: RootState) => state.rack.rack);

/**
 * A custom hook to get the dispatch function, typed for the application's store.
 * @returns The Redux dispatch function.
 */
export const useRackDispatch = (): AppDispatch => useDispatch<AppDispatch>();