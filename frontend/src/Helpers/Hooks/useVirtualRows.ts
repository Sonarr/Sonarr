import { throttle } from 'lodash';
import {
  RefObject,
  useCallback,
  useEffect,
  useLayoutEffect,
  useRef,
  useState,
} from 'react';
import { useAppDimensions } from 'App/appStore';
import { isLocked } from 'Utilities/scrollLock';

const MIN_VIRTUAL_ROWS = 50;
const OVERSCAN_ROWS = 20;
const RANGE_STEP_ROWS = 10;

function useVirtualRows(
  rowCount: number,
  rowHeight: number,
  scrollerRef: RefObject<HTMLElement>,
  isVisible: boolean
) {
  const { height: windowHeight, isSmallScreen } = useAppDimensions();
  const topSpacerRef = useRef<HTMLDivElement>(null);
  const [range, setRange] = useState({
    firstRow: 0,
    lastRow: 0,
  });

  const isVirtual = isVisible && rowCount >= MIN_VIRTUAL_ROWS;

  const updateRange = useCallback(() => {
    const topSpacer = topSpacerRef.current;

    if (!topSpacer || isLocked()) {
      return;
    }

    const top = topSpacer.getBoundingClientRect().top * -1;
    const firstRow = Math.floor(top / rowHeight) - OVERSCAN_ROWS;
    const lastRow = Math.ceil((top + windowHeight) / rowHeight) + OVERSCAN_ROWS;

    const nextLastRow = Math.min(
      rowCount,
      Math.max(0, Math.ceil(lastRow / RANGE_STEP_ROWS) * RANGE_STEP_ROWS)
    );

    const nextRange = {
      firstRow: Math.min(
        nextLastRow,
        Math.max(0, Math.floor(firstRow / RANGE_STEP_ROWS) * RANGE_STEP_ROWS)
      ),
      lastRow: nextLastRow,
    };

    setRange((current) =>
      current.firstRow === nextRange.firstRow &&
      current.lastRow === nextRange.lastRow
        ? current
        : nextRange
    );
  }, [rowCount, rowHeight, windowHeight]);

  useLayoutEffect(() => {
    if (isVirtual) {
      updateRange();
    }
  });

  useEffect(() => {
    if (!isVirtual) {
      return undefined;
    }

    const handleScroll = throttle(updateRange, 10);
    const scrollListener = isSmallScreen ? window : scrollerRef.current;

    scrollListener?.addEventListener('scroll', handleScroll);

    return () => {
      handleScroll.cancel();
      scrollListener?.removeEventListener('scroll', handleScroll);
    };
  }, [isSmallScreen, isVirtual, scrollerRef, updateRange]);

  if (!isVirtual) {
    return {
      topSpacerRef,
      isVirtual,
      startIndex: 0,
      endIndex: rowCount,
      topSpacerHeight: 0,
      bottomSpacerHeight: 0,
    };
  }

  const endIndex = Math.min(range.lastRow, rowCount);
  const startIndex = Math.min(range.firstRow, endIndex);

  return {
    topSpacerRef,
    isVirtual,
    startIndex,
    endIndex,
    topSpacerHeight: startIndex * rowHeight,
    bottomSpacerHeight: (rowCount - endIndex) * rowHeight,
  };
}

export default useVirtualRows;
