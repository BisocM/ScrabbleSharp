/**
 * Safely retrieves and parses a JSON value from localStorage.
 * If the key doesn't exist or parsing fails, it returns a fallback value.
 *
 * @param key The localStorage key to retrieve.
 * @param fallbackValue The value to return in case of an error or if the key is not found.
 * @returns The parsed value from localStorage or the fallback value.
 */
export function safeGet<T>(key: string, fallbackValue: T): T {
    try {
        const rawValue = localStorage.getItem(key);
        return rawValue ? (JSON.parse(rawValue) as T) : fallbackValue;
    } catch {
        return fallbackValue;
    }
}

/**
 * Safely stringifies and sets a value in localStorage.
 * It wraps the operation in a try-catch block to handle potential errors,
 * such as when storage is disabled (e.g., in private Browse).
 *
 * @param key The localStorage key to set.
 * @param value The value to store. It will be JSON.stringified.
 */
export function safeSet<T>(key: string, value: T): void {
    try {
        localStorage.setItem(key, JSON.stringify(value));
    } catch {
        // Silently fail if localStorage is not available.
    }
}