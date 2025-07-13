/* eslint-disable @typescript-eslint/no-var-requires */
const defaultTheme = require("tailwindcss/defaultTheme");
const fs           = require("fs");

/* semantic design tokens */
const palette = {
  surface : "#FFE5C6",
  primary : "#2563EB",
  danger  : "#DC2626",
  bonus2L : "#ABE6FF",
  bonus3L : "#FFD46C",
  bonus2W : "#7DE07A",
  bonus3W : "#FF9E78",
  textHigh: "#1F2937",
  textLow : "#6B7280"
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
        surface : "var(--color-surface)",
        primary : "var(--color-primary)",
        danger  : "var(--color-danger)",
        bonus2L : "var(--color-bonus2L)",
        bonus3L : "var(--color-bonus3L)",
        bonus2W : "var(--color-bonus2W)",
        bonus3W : "var(--color-bonus3W)",
        textHigh: "var(--color-textHigh)",
        textLow : "var(--color-textLow)"
      },
      fontFamily: {
        sans: ["Inter", ...defaultTheme.fontFamily.sans]
      }
    }
  },
  plugins: []
};

//============================================
// WRITE CSS CUSTOM PROPERTIES
//============================================

const css = Object.entries(palette)
    .map(([k, v]) => `  --color-${k}: ${v};`)
    .join("\n");

fs.writeFileSync(
    "./src/theme/variables.css",
    `:root {\n${css}\n}\n`
);