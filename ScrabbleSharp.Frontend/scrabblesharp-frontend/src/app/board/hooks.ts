import { useSelector, useDispatch } from "react-redux";
import type { RootState, AppDispatch } from "@/app/store";

/**
 * A custom hook to access the board state from the Redux store.
 * @returns The current board state.
 */
export const useBoardState = () => useSelector((state: RootState) => state.board);

/**
 * A custom hook to get the dispatch function, typed for the application's store.
 * @returns The Redux dispatch function.
 */
export const useBoardDispatch = (): AppDispatch => useDispatch<AppDispatch>();