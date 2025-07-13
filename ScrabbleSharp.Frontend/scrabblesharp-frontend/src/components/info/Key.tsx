import React from "react";
import clsx from "clsx";

interface Props extends React.HTMLAttributes<HTMLElement> {
    children: React.ReactNode;
    variant?: 'icon' | 'standard';
    as?: 'kbd' | 'span';
}

const Key: React.FC<Props> = ({ children, className, variant = 'standard', as = 'kbd', ...props }) => {
    const Tag = as;
    const baseClasses = "inline-flex items-center justify-center font-sans font-semibold bg-slate-200 dark:bg-slate-700 text-slate-900 dark:text-slate-200 border border-slate-300 dark:border-slate-600 rounded-md shadow-sm";

    const variantClasses = {
        standard: "h-6 px-1.5 min-w-[1.5rem] text-sm",
        icon: "h-6 w-6 p-0",
    };

    return (
        <Tag className={clsx(baseClasses, variantClasses[variant], className)} {...props}>
            {children}
        </Tag>
    );
};

export default Key;