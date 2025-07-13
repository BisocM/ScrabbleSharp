import React, { useEffect } from "react";
import { useSettings, useSettingsDispatch } from "@/app/settings/hooks";
import { toggleTheme } from "@/app/settings/settingsSlice";
import Button from "./Button";

/**
 * A button component to toggle between light and dark themes.
 */
const Toggle: React.FC = () => {
    const { theme } = useSettings();
    const dispatch = useSettingsDispatch();

    // Effect to apply the 'dark' class to the root <html> element when the theme changes.
    // This allows Tailwind's dark mode variants to work.
    useEffect(() => {
        const rootElement = document.documentElement;
        if (theme === "dark") {
            rootElement.classList.add("dark");
        } else {
            rootElement.classList.remove("dark");
        }
    }, [theme]);

    return (
        <Button
            variant="secondary"
            compact
            className="!p-2 flex items-center justify-center w-10 h-10"
            onClick={() => dispatch(toggleTheme())}
            title="Toggle dark mode"
            aria-label="toggle dark mode"
        >
            {theme === "dark" ? "☀️" : "🌙"}
        </Button>
    );
};

export default Toggle;