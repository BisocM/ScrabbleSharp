import React from 'react';
import { letterScores } from '@/data/letterScores';

const Logo: React.FC = () => {
    const line1 = 'Scrabble'.split('');
    const line2 = 'Sharp'.split('');

    const colors = [
        'bg-red-500', 'bg-orange-500', 'bg-amber-500', 'bg-lime-500',
        'bg-green-500', 'bg-emerald-500', 'bg-teal-500', 'bg-cyan-500',
        'bg-sky-500', 'bg-blue-500', 'bg-indigo-500', 'bg-violet-500'
    ];

    const renderLine = (letters: string[], offset: number) => (
        <div className="flex items-center gap-0.5">
            {letters.map((char, index) => {
                const letter = char.toUpperCase();
                const score = letterScores[letter] ?? 0;
                const colorClass = colors[(index + offset) % colors.length];

                return (
                    <div
                        key={index}
                        className={`relative w-5 h-5 flex items-center justify-center rounded-sm shadow-sm select-none ${colorClass}`}
                        title={`${letter}: ${score} points`}
                    >
            <span className="font-bold text-sm text-white" style={{ textShadow: '1px 1px 1px rgba(0,0,0,0.3)' }}>
              {letter}
            </span>
                        <span className="absolute bottom-0 right-0.5 text-[0.5rem] font-bold text-white/90" style={{ textShadow: '1px 1px 1px rgba(0,0,0,0.3)' }}>
              {score}
            </span>
                    </div>
                );
            })}
        </div>
    );

    return (
        <div className="flex flex-col items-center gap-0.5" aria-label="ScrabbleSharp">
            {renderLine(line1, 0)}
            {renderLine(line2, line1.length)}
        </div>
    );
};

export default Logo;