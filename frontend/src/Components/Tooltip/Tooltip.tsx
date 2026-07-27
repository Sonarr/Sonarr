import {
  arrow,
  autoUpdate,
  flip,
  FloatingArrow,
  FloatingFocusManager,
  FloatingPortal,
  offset,
  Placement,
  safePolygon,
  shift,
  useClick,
  useDismiss,
  useFloating,
  useFocus,
  useHover,
  useInteractions,
  useRole,
} from '@floating-ui/react';
import classNames from 'classnames';
import React, { useRef, useState } from 'react';
import { useThemeColor } from 'Helpers/Hooks/useTheme';
import { kinds } from 'Helpers/Props';
import { Kind } from 'Helpers/Props/kinds';
import { isMobile } from 'Utilities/browser';
import styles from './Tooltip.css';

export interface TooltipProps {
  accessibleLabel?: string;
  className?: string;
  bodyClassName?: string;
  anchor: React.ReactNode;
  tooltip: string | React.ReactNode;
  contentRole?: 'dialog' | 'tooltip';
  isAnchorFocusable?: boolean;
  kind?: Extract<Kind, 'default' | 'inverse'>;
  position?: Placement;
  canFlip?: boolean;
}

function Tooltip(props: TooltipProps) {
  const {
    accessibleLabel,
    className,
    bodyClassName = styles.body,
    anchor,
    tooltip,
    contentRole = 'tooltip',
    isAnchorFocusable = true,
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
    enabled: isMobile() || contentRole === 'dialog',
  });
  const dismiss = useDismiss(context);
  const focus = useFocus(context, {
    enabled: contentRole === 'tooltip',
  });
  const hover = useHover(context, {
    handleClose: safePolygon(),
  });
  const role = useRole(context, { role: contentRole });

  const { getReferenceProps, getFloatingProps } = useInteractions([
    click,
    dismiss,
    focus,
    hover,
    role,
  ]);

  const floatingContent = (
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
  );

  return (
    <>
      <span
        ref={refs.setReference}
        {...getReferenceProps({
          'aria-label': accessibleLabel,
          role: contentRole === 'dialog' ? 'button' : undefined,
          tabIndex: isAnchorFocusable ? 0 : undefined,
        })}
        className={classNames(styles.reference, className)}
      >
        {anchor}
      </span>
      {isOpen ? (
        <FloatingPortal id="portal-root">
          {contentRole === 'dialog' ? (
            <FloatingFocusManager
              context={context}
              initialFocus={-1}
              modal={false}
              order={['reference', 'content']}
            >
              {floatingContent}
            </FloatingFocusManager>
          ) : (
            floatingContent
          )}
        </FloatingPortal>
      ) : null}
    </>
  );
}

export default Tooltip;
