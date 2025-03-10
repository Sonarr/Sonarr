import {
  arrow,
  autoUpdate,
  flip,
  FloatingArrow,
  FloatingPortal,
  offset,
  Placement,
  safePolygon,
  shift,
  useClick,
  useDismiss,
  useFloating,
  useHover,
  useInteractions,
} from '@floating-ui/react';
import classNames from 'classnames';
import React, { useRef, useState } from 'react';
import { useThemeColor } from 'Helpers/Hooks/useTheme';
import { kinds } from 'Helpers/Props';
import { Kind } from 'Helpers/Props/kinds';
import { isMobile } from 'Utilities/browser';
import styles from './Tooltip.css';

export interface TooltipProps {
  className?: string;
  bodyClassName?: string;
  anchor: React.ReactNode;
  tooltip: string | React.ReactNode;
  kind?: Extract<Kind, 'default' | 'inverse'>;
  position?: Placement;
  canFlip?: boolean;
}
function Tooltip(props: TooltipProps) {
  const {
    className,
    bodyClassName = styles.body,
    anchor,
    tooltip,
    kind = kinds.DEFAULT,
    position,
    canFlip = true,
  } = props;

  const arrowColor = useThemeColor(
    kind === 'inverse'
      ? 'popoverArrowBorderInverseColor'
      : 'popoverArrowBorderColor'
  );
  const [isOpen, setIsOpen] = useState(false);

  const arrowRef = useRef(null);

  const { refs, context, floatingStyles } = useFloating({
    middleware: [
      arrow({
        element: arrowRef,
      }),
      flip({
        crossAxis: canFlip,
        mainAxis: canFlip,
      }),
      offset({ mainAxis: 10 }),
      shift(),
    ],
    open: isOpen,
    placement: position,
    whileElementsMounted: autoUpdate,
    onOpenChange: setIsOpen,
  });

  const click = useClick(context, {
    enabled: isMobile(),
  });
  const dismiss = useDismiss(context);
  const hover = useHover(context, {
    handleClose: safePolygon(),
  });

  const { getReferenceProps, getFloatingProps } = useInteractions([
    click,
    dismiss,
    hover,
  ]);

  return (
    <>
      <span
        ref={refs.setReference}
        {...getReferenceProps()}
        className={className}
      >
        {anchor}
      </span>
      {isOpen ? (
        <FloatingPortal id="portal-root">
          <div
            ref={refs.setFloating}
            className={styles.tooltipContainer}
            style={floatingStyles}
            {...getFloatingProps()}
          >
            <FloatingArrow ref={arrowRef} context={context} fill={arrowColor} />
            <div className={classNames(styles.tooltip, styles[kind])}>
              <div className={bodyClassName}>{tooltip}</div>
            </div>
          </div>
        </FloatingPortal>
      ) : null}
    </>
  );
}

export default Tooltip;
