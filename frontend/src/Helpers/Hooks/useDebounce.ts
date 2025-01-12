import { useEffect, useRef, useState } from 'react';

function useDebounce<T>(value: T, delay: number) {
  const [debouncedValue, setDebouncedValue] = useState<T>(value);
  const valueTimeout = useRef<ReturnType<typeof setTimeout>>();

  useEffect(() => {
    if (delay === 0) {
      setDebouncedValue(value);
      clearTimeout(valueTimeout.current);
      return;
    }

    valueTimeout.current = setTimeout(() => {
      setDebouncedValue(value);
    }, delay);

    return () => {
      clearTimeout(valueTimeout.current);
    };
  }, [value, delay]);

  return debouncedValue;
}

export default useDebounce;
