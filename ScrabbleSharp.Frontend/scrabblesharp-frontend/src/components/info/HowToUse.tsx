import React from "react";
import Key from "./Key";

const ArrowIcon = ({ d }: { d: string }) => (
    <svg xmlns="http://www.w3.org/2000/svg" className="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2.5}>
        <path strokeLinecap="round" strokeLinejoin="round" d={d} />
    </svg>
);

const MouseIcon = ({ highlight }: { highlight: 'scroll' | 'middle' }) => (
    <svg className="w-6 h-6" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
        <path d="M12 2C8.13401 2 5 5.13401 5 9V15C5 18.866 8.13401 22 12 22C15.866 22 19 18.866 19 15V9C19 5.13401 15.866 2 12 2Z"
              className="stroke-current text-slate-500 dark:text-slate-400" strokeWidth="1.5" />
        <path d="M11 6H13V11H11V6Z"
              className={highlight === 'scroll' ? "fill-blue-500" : "fill-current text-slate-500 dark:text-slate-400"} />
        <path d="M11 13H13V18H11V13Z"
              className={highlight === 'middle' ? "fill-blue-500" : "fill-current text-slate-500 dark:text-slate-400"} />
    </svg>
);

const BackspaceIcon = () => (
    <svg className="w-5 h-5" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
        <path d="M21 12H6.414L10.707 7.707C11.098 7.316 11.098 6.684 10.707 6.293C10.316 5.902 9.684 5.902 9.293 6.293L3.293 12.293C2.902 12.684 2.902 13.316 3.293 13.707L9.293 19.707C9.684 20.098 10.316 20.098 10.707 19.707C11.098 19.316 11.098 18.684 10.707 18.293L6.414 14H21C21.552 14 22 13.552 22 13C22 12.448 21.552 12 21 12Z"
              className="fill-current text-slate-600 dark:text-slate-300" />
    </svg>
);

const HowToUse: React.FC = () => (
    <div className="p-4 bg-white dark:bg-slate-800 rounded-lg shadow-lg">
        <h3 className="text-lg font-bold mb-4 text-slate-800 dark:text-slate-200">Controls</h3>
        <ul className="space-y-3 text-sm text-slate-600 dark:text-slate-400">
            <li className="flex items-center justify-between">
                <span>Pan Board</span>
                <div className="flex flex-col items-end">
                    <Key as="span" className="p-1 pr-2 flex items-center gap-1.5">
                        <MouseIcon highlight="middle" />
                        <span>Drag</span>
                    </Key>
                    <span className="text-xs mt-1 text-slate-500 dark:text-slate-400">or Two-finger Drag</span>
                </div>
            </li>
            <li className="flex items-center justify-between">
                <span>Zoom Board</span>
                <div className="flex flex-col items-end">
                    <Key as="span" className="p-1 pr-2 flex items-center gap-1.5">
                        <MouseIcon highlight="scroll" />
                        <span>Scroll</span>
                    </Key>
                    <span className="text-xs mt-1 text-slate-500 dark:text-slate-400">or Pinch</span>
                </div>
            </li>
            <li className="flex items-center justify-between">
                <span>Change Direction</span>
                <div className="flex items-center gap-1">
                    <Key variant="icon"><ArrowIcon d="M15 19l-7-7 7-7" /></Key>
                    <Key variant="icon"><ArrowIcon d="M5 15l7-7 7 7" /></Key>
                    <Key variant="icon"><ArrowIcon d="M19 9l-7 7-7-7" /></Key>
                    <Key variant="icon"><ArrowIcon d="M9 5l7 7-7 7" /></Key>
                </div>
            </li>
            <li className="flex items-center justify-between">
                <span>Enter Blank</span>
                <div className="flex items-center gap-1.5">
                    <Key>?</Key><span>/</span><Key>_</Key><span>+</span><Key>A</Key>
                </div>
            </li>
            <li className="flex items-center justify-between">
                <span>Delete Tile</span>
                <Key className="px-2 w-28 flex justify-between items-center">
                    <span className="text-xs">Backspace</span>
                    <BackspaceIcon />
                </Key>
            </li>
        </ul>
    </div>
);

export default HowToUse;