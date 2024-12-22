import React, { CSSProperties, LegacyRef, useId } from 'react';
import Scroller from 'Components/Scroller/Scroller';
import styles from './MenuContent.css';

interface MenuContentProps {
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
}: MenuContentProps) {
  const generatedId = useId();

  return (
    <div
      ref={forwardedRef}
      id={id ?? generatedId}
      className={className}
      style={style}
    >
      {isOpen ? (
        <Scroller className={styles.scroller}>{children}</Scroller>
      ) : null}
    </div>
  );
}

export default MenuContent;
