import { useEffect, useState } from 'react';

/**
 * A custom React hook that persists state to `localStorage`.
 * It behaves like `useState` but automatically saves the value to localStorage
 * on every change and loads the initial value from localStorage on mount.
 *
 * @param key The key to use for storing the value in localStorage.
 * @param initialValue The initial value to use if no value is found in localStorage.
 * @returns A stateful value and a function to update it, just like `useState`.
 */
export default function usePersistedState<T>(key: string, initialValue: T) {
    const [value, setValue] = useState<T>(() => {
        try {
            const rawValue = localStorage.getItem(key);
            return rawValue ? (JSON.parse(rawValue) as T) : initialValue;
        } catch {
            // If JSON parsing fails, return the initial value.
            return initialValue;
        }
    });

    useEffect(() => {
        try {
            localStorage.setItem(key, JSON.stringify(value));
        } catch {
            // Ignore potential storage errors (e.g., private Browse mode).
        }
    }, [key, value]);

    return [value, setValue] as const;
}