import React, { useEffect, ReactNode, MouseEvent as ReactMouseEvent } from "react";
import Button from "./Button";

interface Props {
    show: boolean;
    title: string;
    onClose: () => void;
    children: ReactNode;
    width?: string; // tailwind max-w class like `max-w-sm`
}

/**
 * A modal dialog component that overlays the page content.
 */
const Modal: React.FC<Props> = ({
                                    show,
                                    title,
                                    onClose,
                                    children,
                                    width = "max-w-sm"
                                }) => {
    // Effect to add/remove a keydown listener for the 'Escape' key to close the modal.
    useEffect(() => {
        if (!show) return;
        const escapeKeyHandler = (event: KeyboardEvent) => {
            if (event.key === "Escape") onClose();
        };
        window.addEventListener("keydown", escapeKeyHandler);
        return () => window.removeEventListener("keydown", escapeKeyHandler);
    }, [show, onClose]);

    if (!show) return null;

    return (
        // The backdrop overlay
        <div
            className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 backdrop-blur-sm"
            onClick={onClose}
        >
            {/* The modal content */}
            <div
                className={`bg-white dark:bg-slate-800 p-6 rounded-lg shadow-xl w-full ${width}`}
                onClick={(event: ReactMouseEvent) => event.stopPropagation()} // Prevent clicks inside the modal from closing it
            >
                <div className="flex items-center justify-between mb-4">
                    <h2 className="text-xl font-bold">{title}</h2>
                    <Button variant="ghost" compact onClick={onClose} aria-label="close">
                        ×
                    </Button>
                </div>
                {children}
            </div>
        </div>
    );
};

export default Modal;