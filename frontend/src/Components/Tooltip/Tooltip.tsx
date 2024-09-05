import classNames from 'classnames';
import React, {
  useCallback,
  useEffect,
  useMemo,
  useRef,
  useState,
} from 'react';
import { Manager, Popper, Reference } from 'react-popper';
import Portal from 'Components/Portal';
import { kinds, tooltipPositions } from 'Helpers/Props';
import { Kind } from 'Helpers/Props/kinds';
import dimensions from 'Styles/Variables/dimensions';
import { isMobile as isMobileUtil } from 'Utilities/browser';
import styles from './Tooltip.css';

export interface TooltipProps {
  className?: string;
  bodyClassName?: string;
  anchor: React.ReactNode;
  tooltip: string | React.ReactNode;
  kind?: Extract<Kind, keyof typeof styles>;
  position?: (typeof tooltipPositions.all)[number];
  canFlip?: boolean;
}
function Tooltip(props: TooltipProps) {
  const {
    className,
    bodyClassName = styles.body,
    anchor,
    tooltip,
    kind = kinds.DEFAULT,
    position = tooltipPositions.TOP,
    canFlip = false,
  } = props;

  const closeTimeout = useRef<ReturnType<typeof setTimeout>>();
  const updater = useRef<(() => void) | null>(null);
  const [isOpen, setIsOpen] = useState(false);

  const handleClick = useCallback(() => {
    if (!isMobileUtil()) {
      return;
    }

    setIsOpen((isOpen) => {
      return !isOpen;
    });
  }, [setIsOpen]);

  const handleMouseEnterAnchor = useCallback(() => {
    // Mobile will fire mouse enter and click events rapidly,
    // this causes the tooltip not to open on the first press.
    // Ignore the mouse enter event on mobile.

    if (isMobileUtil()) {
      return;
    }

    if (closeTimeout.current) {
      clearTimeout(closeTimeout.current);
    }

    setIsOpen(true);
  }, [setIsOpen]);

  const handleMouseEnterTooltip = useCallback(() => {
    if (closeTimeout.current) {
      clearTimeout(closeTimeout.current);
    }

    setIsOpen(true);
  }, [setIsOpen]);

  const handleMouseLeave = useCallback(() => {
    // Still listen for mouse leave on mobile to allow clicks outside to close the tooltip.

    clearTimeout(closeTimeout.current);
    closeTimeout.current = setTimeout(() => {
      setIsOpen(false);
    }, 100);
  }, [setIsOpen]);

  const maxWidth = useMemo(() => {
    const windowWidth = window.innerWidth;

    if (windowWidth >= parseInt(dimensions.breakpointLarge)) {
      return 800;
    } else if (windowWidth >= parseInt(dimensions.breakpointMedium)) {
      return 650;
    } else if (windowWidth >= parseInt(dimensions.breakpointSmall)) {
      return 500;
    }

    return 450;
  }, []);

  const computeMaxSize = useCallback(
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    (data: any) => {
      const { top, right, bottom, left } = data.offsets.reference;

      const windowWidth = window.innerWidth;
      const windowHeight = window.innerHeight;

      if (/^top/.test(data.placement)) {
        data.styles.maxHeight = top - 20;
      } else if (/^bottom/.test(data.placement)) {
        data.styles.maxHeight = windowHeight - bottom - 20;
      } else if (/^right/.test(data.placement)) {
        data.styles.maxWidth = Math.min(maxWidth, windowWidth - right - 20);
        data.styles.maxHeight = top - 20;
      } else {
        data.styles.maxWidth = Math.min(maxWidth, left - 20);
        data.styles.maxHeight = top - 20;
      }

      return data;
    },
    [maxWidth]
  );

  useEffect(() => {
    if (updater.current && isOpen) {
      updater.current();
    }
  });

  useEffect(() => {
    return () => {
      if (closeTimeout.current) {
        clearTimeout(closeTimeout.current);
      }
    };
  }, []);

  return (
    <Manager>
      <Reference>
        {({ ref }) => (
          <span
            ref={ref}
            className={className}
            onClick={handleClick}
            onMouseEnter={handleMouseEnterAnchor}
            onMouseLeave={handleMouseLeave}
          >
            {anchor}
          </span>
        )}
      </Reference>

      <Portal>
        <Popper
          // @ts-expect-error - PopperJS types are not in sync with our position types.
          placement={position}
          // Disable events to improve performance when many tooltips
          // are shown (Quality Definitions for example).
          eventsEnabled={false}
          modifiers={{
            computeMaxHeight: {
              order: 851,
              enabled: true,
              fn: computeMaxSize,
            },
            preventOverflow: {
              // Fixes positioning for tooltips in the queue
              // and likely others.
              escapeWithReference: false,
            },
            flip: {
              enabled: canFlip,
            },
          }}
        >
          {({ ref, style, placement, arrowProps, scheduleUpdate }) => {
            updater.current = scheduleUpdate;

            const popperPlacement = placement
              ? placement.split('-')[0]
              : position;
            const vertical =
              popperPlacement === 'top' || popperPlacement === 'bottom';

            return (
              <div
                ref={ref}
                className={classNames(
                  styles.tooltipContainer,
                  vertical
                    ? styles.verticalContainer
                    : styles.horizontalContainer
                )}
                style={style}
                onMouseEnter={handleMouseEnterTooltip}
                onMouseLeave={handleMouseLeave}
              >
                <div
                  ref={arrowProps.ref}
                  className={
                    isOpen
                      ? classNames(
                          styles.arrow,
                          styles[kind],
                          // @ts-expect-error - is a string that may not exist in styles
                          styles[popperPlacement]
                        )
                      : styles.arrowDisabled
                  }
                  style={arrowProps.style}
                />
                {isOpen ? (
                  <div className={classNames(styles.tooltip, styles[kind])}>
                    <div className={bodyClassName}>{tooltip}</div>
                  </div>
                ) : null}
              </div>
            );
          }}
        </Popper>
      </Portal>
    </Manager>
  );
}

export default Tooltip;
