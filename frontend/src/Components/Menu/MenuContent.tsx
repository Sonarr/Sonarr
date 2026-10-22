import React, { CSSProperties, HTMLAttributes, LegacyRef, useId } from 'react';
import Scroller from 'Components/Scroller/Scroller';
import styles from './MenuContent.module.css';

interface MenuContentProps extends HTMLAttributes<HTMLDivElement> {
  forwardedRef?: LegacyRef<HTMLDivElement> | undefined;
  className?: string;
  id?: string;
  children: React.ReactNode;
  style?: CSSProperties;
  isOpen?: boolean;
}

function MenuContent({
  forwardedRef,
  className = styles.menuContent,
  id,
  children,
  style,
  isOpen,
  ...otherProps
}: MenuContentProps) {
  const generatedId = useId();

  return (
    <div
      ref={forwardedRef}
      id={id ?? generatedId}
      className={className}
      style={style}
      {...otherProps}
    >
      {isOpen ? (
        <Scroller className={styles.scroller}>{children}</Scroller>
      ) : null}
    </div>
  );
}

export default MenuContent;
