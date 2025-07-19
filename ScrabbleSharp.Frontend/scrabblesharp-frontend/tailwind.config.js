const defaultTheme = require("tailwindcss/defaultTheme");
const fs = require("fs");

const lightPalette = {
    surface: "#ffffff",
    surfaceInteractive: "#f1f5f9",
    border: "#e2e8f0",
    textHigh: "#1e293b",
    textLow: "#64748b",
    primary: "#16a34a",
    danger: "#ef4444",
    bonus2L: "#bae6fd",
    bonus3L: "#60a5fa",
    bonus2W: "#fecaca",
    bonus3W: "#f87171",
};

const darkPalette = {
    surface: "#0f172a",
    surfaceInteractive: "#1e293b",
    border: "#334155",
    textHigh: "#f1f5f9",
    textLow: "#94a3b8",
    primary: "#22c55e",
    danger: "#f87171",
};

module.exports = {
    content: [
        "./index.html",
        "./src/**/*.{js,ts,jsx,tsx}"
    ],
    darkMode: "class",
    theme: {
        extend: {
            colors: {
                surface: "var(--color-surface)",
                surfaceInteractive: "var(--color-surface-interactive)",
                border: "var(--color-border)",
                textHigh: "var(--color-text-high)",
                textLow: "var(--color-text-low)",
                primary: "var(--color-primary)",
                danger: "var(--color-danger)",
                bonus2L: "var(--color-bonus-2l)",
                bonus3L: "var(--color-bonus-3l)",
                bonus2W: "var(--color-bonus-2w)",
                bonus3W: "var(--color-bonus-3w)",
            },
            fontFamily: {
                sans: ["Inter", ...defaultTheme.fontFamily.sans],
            },
        },
    },
    plugins: [],
};

// Converts a camelCase string to kebab-case
function camelToKebab(str) {
    return str.replace(/([a-z0-9]|(?=[A-Z]))([A-Z])/g, '$1-$2').toLowerCase();
}

// Generates CSS variables from a palette object
function makeCssVars(obj) {
    return Object.entries(obj)
        .map(([key, value]) => `    --color-${camelToKebab(key)}: ${value};`)
        .join("\n");
}

const lightCss = makeCssVars(lightPalette);
const darkCss = makeCssVars(darkPalette);

const fullCss = `:root {\n${lightCss}\n}\n\n:root.dark {\n${darkCss}\n}\n`;

fs.writeFileSync(
    "./src/theme/variables.css",
    fullCss
);