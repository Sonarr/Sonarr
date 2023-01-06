import React, { forwardRef, ReactNode, useCallback } from 'react';
import Scroller from 'Components/Scroller/Scroller';
import ScrollDirection from 'Helpers/Props/ScrollDirection';
import { isLocked } from 'Utilities/scrollLock';
import styles from './PageContentBody.css';

interface PageContentBodyProps {
  className: string;
  innerClassName: string;
  children: ReactNode;
  initialScrollTop?: number;
  onScroll?: (payload) => void;
}

const PageContentBody = forwardRef(
  (
    props: PageContentBodyProps,
    ref: React.MutableRefObject<HTMLDivElement>
  ) => {
    const {
      className = styles.contentBody,
      innerClassName = styles.innerContentBody,
      children,
      onScroll,
      ...otherProps
    } = props;

    const onScrollWrapper = useCallback(
      (payload) => {
        if (onScroll && !isLocked()) {
          onScroll(payload);
        }
      },
      [onScroll]
    );

    return (
      <Scroller
        ref={ref}
        {...otherProps}
        className={className}
        scrollDirection={ScrollDirection.Vertical}
        onScroll={onScrollWrapper}
      >
        <div className={innerClassName}>{children}</div>
      </Scroller>
    );
  }
);

export default PageContentBody;
