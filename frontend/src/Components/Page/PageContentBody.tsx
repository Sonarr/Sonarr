import React, { ForwardedRef, forwardRef, ReactNode, useCallback } from 'react';
import Scroller, { OnScroll } from 'Components/Scroller/Scroller';
import useScrollPosition from 'Helpers/Hooks/useScrollPosition';
import { isLocked } from 'Utilities/scrollLock';
import styles from './PageContentBody.css';

interface PageContentBodyProps {
  className?: string;
  innerClassName?: string;
  children: ReactNode;
  scrollPositionKey?: string;
  onScroll?: (payload: OnScroll) => void;
}

const PageContentBody = forwardRef(
  (props: PageContentBodyProps, ref: ForwardedRef<HTMLDivElement>) => {
    const {
      className = styles.contentBody,
      innerClassName = styles.innerContentBody,
      children,
      scrollPositionKey,
      onScroll,
    } = props;

    const { initialScrollTop, onScroll: onScrollMemo } =
      useScrollPosition(scrollPositionKey);

    const handleScroll = useCallback(
      (payload: OnScroll) => {
        if (isLocked()) {
          return;
        }

        onScrollMemo(payload);
        onScroll?.(payload);
      },
      [onScroll, onScrollMemo]
    );

    return (
      <Scroller
        ref={ref}
        className={className}
        scrollDirection="vertical"
        initialScrollTop={initialScrollTop}
        onScroll={handleScroll}
      >
        <div className={innerClassName}>{children}</div>
      </Scroller>
    );
  }
);

export default PageContentBody;
