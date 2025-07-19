import {useRef, useEffect} from 'react';

// This hook stores a value from the previous render cycle.
export function usePrevious<T>(value: T): T | undefined {
    const ref = useRef<T>();
    useEffect(() => {
        ref.current = value;
    }, [value]);
    return ref.current;
}