import React, { ReactElement } from "react";

interface Props {
    text: string;
    children: ReactElement;
}

/**
 * A simple CSS-based tooltip component. The tooltip text appears when the
 * user hovers over the child element. This is achieved using the `group`
 * and `group-hover` utility classes from Tailwind CSS.
 */
const Tooltip: React.FC<Props> = ({ text, children }) => (
    <span className="relative group">
        {children}
        <span
            className="opacity-0 group-hover:opacity-100 transition-opacity
                       absolute bottom-full left-1/2 -translate-x-1/2 mb-1
                       bg-black text-white text-xs rounded px-2 py-1 whitespace-nowrap"
        >
            {text}
        </span>
    </span>
);

export default Tooltip;