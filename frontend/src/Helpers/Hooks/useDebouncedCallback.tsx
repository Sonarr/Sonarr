import { debounce, DebouncedFunc, DebounceSettings } from 'lodash';
import { useCallback } from 'react';

// eslint-disable-next-line @typescript-eslint/no-explicit-any
export default function useDebouncedCallback<T extends (...args: any) => any>(
  callback: T,
  delay: number,
  options?: DebounceSettings
): DebouncedFunc<T> {
  // eslint-disable-next-line react-hooks/exhaustive-deps
  return useCallback(debounce(callback, delay, options), [
    callback,
    delay,
    options,
  ]);
}
