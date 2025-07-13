/**
 * Converts the rack array into a single string format expected by the backend.
 * Blank tiles, represented as '_' in the frontend, are kept as '_'.
 * All letters are converted to uppercase.
 *
 * @param rack The player's rack as an array of characters.
 * @returns A string representation of the rack.
 */
export function rackToString(rack: string[]): string {
    return rack.map(character => (character === "?" ? "_" : character).toUpperCase()).join("");
}