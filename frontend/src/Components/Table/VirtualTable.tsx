import { throttle } from 'lodash';
import React, { RefObject, useEffect, useState } from 'react';
import { FixedSizeList, ListChildComponentProps } from 'react-window';
import Scroller from 'Components/Scroller/Scroller';
import useMeasure from 'Helpers/Hooks/useMeasure';
import dimensions from 'Styles/Variables/dimensions';
import styles from './VirtualTable.css';

const bodyPadding = parseInt(dimensions.pageContentBodyPadding);
const bodyPaddingSmallScreen = parseInt(
  dimensions.pageContentBodyPaddingSmallScreen
);

interface VirtualTableProps<T> {
  Header: React.JSX.Element;
  itemCount: number;
  itemData: T;
  isSmallScreen: boolean;
  listRef: RefObject<FixedSizeList<T>>;
  rowHeight: number;
  Row({
    index,
    style,
    data,
  }: ListChildComponentProps<T>): React.JSX.Element | null;
  scrollerRef: RefObject<HTMLElement>;
}

function getWindowScrollTopPosition() {
  return document.documentElement.scrollTop || document.body.scrollTop || 0;
}

function VirtualTable<T>({
  Header,
  itemCount,
  itemData,
  isSmallScreen,
  listRef,
  rowHeight,
  Row,
  scrollerRef,
}: VirtualTableProps<T>) {
  const [measureRef, bounds] = useMeasure();
  const [size, setSize] = useState({ width: 0, height: 0 });
  const windowWidth = window.innerWidth;
  const windowHeight = window.innerHeight;

  useEffect(() => {
    const current = scrollerRef?.current as HTMLElement;

    if (isSmallScreen) {
      setSize({
        width: windowWidth,
        height: windowHeight,
      });

      return;
    }

    if (current) {
      const width = current.clientWidth;
      const padding =
        (isSmallScreen ? bodyPaddingSmallScreen : bodyPadding) - 5;

      setSize({
        width: width - padding * 2,
        height: windowHeight,
      });
    }
  }, [isSmallScreen, windowWidth, windowHeight, scrollerRef, bounds]);

  useEffect(() => {
    const currentScrollerRef = scrollerRef.current as HTMLElement;
    const currentScrollListener = isSmallScreen ? window : currentScrollerRef;

    const handleScroll = throttle(() => {
      const { offsetTop = 0 } = currentScrollerRef;
      const scrollTop =
        (isSmallScreen
          ? getWindowScrollTopPosition()
          : currentScrollerRef.scrollTop) - offsetTop;

      listRef.current?.scrollTo(scrollTop);
    }, 10);

    currentScrollListener.addEventListener('scroll', handleScroll);

    return () => {
      handleScroll.cancel();

      if (currentScrollListener) {
        currentScrollListener.removeEventListener('scroll', handleScroll);
      }
    };
  }, [isSmallScreen, listRef, scrollerRef]);

  return (
    <div ref={measureRef}>
      <Scroller className={styles.tableScroller} scrollDirection="horizontal">
        {Header}
        <FixedSizeList<T>
          ref={listRef}
          style={{
            width: '100%',
            height: '100%',
            overflow: 'none',
          }}
          width={size.width}
          height={size.height}
          itemCount={itemCount}
          itemSize={rowHeight}
          itemData={itemData}
          overscanCount={20}
        >
          {Row}
        </FixedSizeList>
      </Scroller>
    </div>
  );
}

export default VirtualTable;
