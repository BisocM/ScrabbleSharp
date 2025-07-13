import { useSelector, useDispatch } from "react-redux";
import type { RootState, AppDispatch } from "@/app/store";

/**
 * A custom hook to access the application settings from the Redux store.
 * @returns The current settings state.
 */
export const useSettings = () => useSelector((state: RootState) => state.settings);

/**
 * A custom hook to get the dispatch function, typed for the application's store.
 * @returns The Redux dispatch function.
 */
export const useSettingsDispatch = (): AppDispatch => useDispatch<AppDispatch>();