import React from "react";
import clsx from "clsx";

// Defines the visual styles a button can have.
type Variant = "primary" | "secondary" | "danger" | "ghost";

interface Props extends React.ButtonHTMLAttributes<HTMLButtonElement> {
    variant?: Variant;
    compact?: boolean;
}

// Maps variant names to their corresponding CSS class names defined in `theme/index.css`.
const variantStyles: Record<Variant, string> = {
    primary: "btn-primary",
    secondary: "btn-secondary",
    danger: "btn-danger",
    ghost: "btn"
};

/**
 * A general-purpose button component with different visual variants.
 */
const Button: React.FC<Props> = ({
                                     variant = "primary",
                                     compact = false,
                                     className,
                                     ...rest
                                 }) => (
    <button
        className={clsx(
            variantStyles[variant],
            compact && "px-2 py-1 text-xs", // Apply compact styling if needed
            className
        )}
        {...rest}
    />
);

export default Button;