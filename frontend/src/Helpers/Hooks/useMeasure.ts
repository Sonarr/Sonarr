import { ResizeObserver as ResizeObserverPolyfill } from '@juggle/resize-observer';
import {
  default as useMeasureHook,
  Options,
  RectReadOnly,
} from 'react-use-measure';

const ResizeObserver = window.ResizeObserver || ResizeObserverPolyfill;

export type Measurements = RectReadOnly;

function useMeasure(
  options?: Omit<Options, 'polyfill'>
): ReturnType<typeof useMeasureHook> {
  return useMeasureHook({
    polyfill: ResizeObserver,
    ...options,
  });
}

export default useMeasure;
