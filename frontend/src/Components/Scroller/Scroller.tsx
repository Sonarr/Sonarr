import classNames from 'classnames';
import { throttle } from 'lodash';
import React, {
  ComponentProps,
  ForwardedRef,
  forwardRef,
  ReactNode,
  useCallback,
  useEffect,
  useRef,
} from 'react';
import { ScrollDirection } from 'Helpers/Props/scrollDirections';
import styles from './Scroller.css';

export interface OnScroll {
  scrollLeft: number;
  scrollTop: number;
}

interface ScrollerProps
  extends Omit<ComponentProps<'div'>, 'children' | 'onScroll'> {
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
      scrollDirection = 'vertical',
      children,
      scrollTop,
      initialScrollTop,
      onScroll,
      tabIndex = -1,
      ...otherProps
    } = props;

    const internalRef = useRef<HTMLDivElement | null>(null);
    const setRef = useCallback(
      (node: HTMLDivElement | null) => {
        internalRef.current = node;

        if (typeof ref === 'function') {
          ref(node);
        } else if (ref) {
          ref.current = node;
        }
      },
      [ref]
    );

    useEffect(
      () => {
        if (initialScrollTop != null && internalRef.current) {
          internalRef.current.scrollTop = initialScrollTop;
        }
      },
      // eslint-disable-next-line react-hooks/exhaustive-deps
      []
    );

    useEffect(() => {
      if (!internalRef.current) {
        return;
      }

      if (scrollTop != null) {
        internalRef.current.scrollTop = scrollTop;
      }

      if (autoFocus && scrollDirection !== 'none') {
        internalRef.current.focus({ preventScroll: true });
      }
    }, [autoFocus, scrollDirection, scrollTop]);

    useEffect(() => {
      const div = internalRef.current;

      if (!div || !onScroll) {
        return undefined;
      }

      const handleScroll = throttle(() => {
        onScroll({
          scrollLeft: div.scrollLeft,
          scrollTop: div.scrollTop,
        });
      }, 10);

      div.addEventListener('scroll', handleScroll);

      return () => {
        div.removeEventListener('scroll', handleScroll);
        handleScroll.cancel();
      };
    }, [onScroll]);

    return (
      <div
        {...otherProps}
        ref={setRef}
        className={classNames(
          className,
          styles.scroller,
          styles[scrollDirection],
          autoScroll && styles.autoScroll
        )}
        tabIndex={tabIndex}
      >
        {children}
      </div>
    );
  }
);

Scroller.displayName = 'Scroller';

export default Scroller;
