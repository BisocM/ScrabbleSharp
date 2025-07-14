/* eslint-disable @typescript-eslint/no-var-requires */
const defaultTheme = require("tailwindcss/defaultTheme");
const fs           = require("fs");

/**
 * Light-mode design tokens
 */
const lightPalette = {
  surface:               "#ffffff",
  surfaceInteractive:   "#f1f5f9",
  border:                "#e2e8f0",
  textHigh:              "#1e293b",
  textLow:               "#64748b",
  primary:               "#16a34a",
  danger:                "#ef4444",
  bonus2L:               "#bae6fd",
  bonus3L:               "#60a5fa",
  bonus2W:               "#fecaca",
  bonus3W:               "#f87171",
};

/**
 * Dark-mode design tokens
 */
const darkPalette = {
  surface:               "#0f172a",
  surfaceInteractive:    "#1e293b",
  border:                "#334155",
  textHigh:              "#f1f5f9",
  textLow:               "#94a3b8",
  primary:               "#22c55e",
  danger:                "#f87171",
  // bonus2L, bonus3L, bonus2W, bonus3W will inherit the light values
};

/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./index.html",
    "./src/**/*.{ts,tsx}"
  ],
  darkMode: "class",
  theme: {
    extend: {
      colors: {
        surface:               "var(--color-surface)",
        surfaceInteractive:    "var(--color-surface-interactive)",
        border:                "var(--color-border)",
        textHigh:              "var(--color-textHigh)",
        textLow:               "var(--color-textLow)",
        primary:               "var(--color-primary)",
        danger:                "var(--color-danger)",
        bonus2L:               "var(--color-bonus2L)",
        bonus3L:               "var(--color-bonus3L)",
        bonus2W:               "var(--color-bonus2W)",
        bonus3W:               "var(--color-bonus3W)",
      },
      fontFamily: {
        sans: ["Inter", ...defaultTheme.fontFamily.sans],
      },
    },
  },
  plugins: [],
};

// ============================================
// WRITE CSS CUSTOM PROPERTIES FOR BOTH MODES
// ============================================
function makeCssVars(obj) {
  return Object.entries(obj)
      .map(([key, value]) => `  --color-${key}: ${value};`)
      .join("\n");
}

const lightCss = makeCssVars(lightPalette);
const darkCss  = makeCssVars(darkPalette);

const fullCss = `:root {\n${lightCss}\n}\n\n:root.dark {\n${darkCss}\n}\n`;

fs.writeFileSync(
    "./src/theme/variables.css",
    fullCss
);
