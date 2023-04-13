import classNames from 'classnames';
import { throttle } from 'lodash';
import React, {
  ForwardedRef,
  forwardRef,
  MutableRefObject,
  ReactNode,
  useEffect,
  useRef,
} from 'react';
import ScrollDirection from 'Helpers/Props/ScrollDirection';
import styles from './Scroller.css';

export interface OnScroll {
  scrollLeft: number;
  scrollTop: number;
}

interface ScrollerProps {
  className?: string;
  scrollDirection?: ScrollDirection;
  autoFocus?: boolean;
  autoScroll?: boolean;
  scrollTop?: number;
  initialScrollTop?: number;
  children?: ReactNode;
  onScroll?: (payload: OnScroll) => void;
}

const Scroller = forwardRef(
  (props: ScrollerProps, ref: ForwardedRef<HTMLDivElement>) => {
    const {
      className,
      autoFocus = false,
      autoScroll = true,
      scrollDirection = ScrollDirection.Vertical,
      children,
      scrollTop,
      initialScrollTop,
      onScroll,
      ...otherProps
    } = props;

    const internalRef = useRef();
    const currentRef = (ref as MutableRefObject<HTMLDivElement>) ?? internalRef;

    useEffect(
      () => {
        if (initialScrollTop != null) {
          currentRef.current.scrollTop = initialScrollTop;
        }
      },
      // eslint-disable-next-line react-hooks/exhaustive-deps
      []
    );

    useEffect(() => {
      if (scrollTop != null) {
        currentRef.current.scrollTop = scrollTop;
      }

      if (autoFocus && scrollDirection !== ScrollDirection.None) {
        currentRef.current.focus({ preventScroll: true });
      }
    }, [autoFocus, currentRef, scrollDirection, scrollTop]);

    useEffect(() => {
      const div = currentRef.current;

      const handleScroll = throttle(() => {
        const scrollLeft = div.scrollLeft;
        const scrollTop = div.scrollTop;

        onScroll?.({ scrollLeft, scrollTop });
      }, 10);

      div.addEventListener('scroll', handleScroll);

      return () => {
        div.removeEventListener('scroll', handleScroll);
      };
    }, [currentRef, onScroll]);

    return (
      <div
        {...otherProps}
        ref={currentRef}
        className={classNames(
          className,
          styles.scroller,
          styles[scrollDirection],
          autoScroll && styles.autoScroll
        )}
        tabIndex={-1}
      >
        {children}
      </div>
    );
  }
);

export default Scroller;
