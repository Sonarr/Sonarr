import React, { CSSProperties, useId } from 'react';
import Scroller from 'Components/Scroller/Scroller';
import styles from './MenuContent.css';

interface MenuContentProps extends React.HTMLAttributes<HTMLDivElement> {
  forwardedRef?: React.Ref<HTMLDivElement>;
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
  isOpen = false,
  ...otherProps
}: MenuContentProps) {
  const generatedId = useId();

  return (
    <div
      ref={forwardedRef}
      id={id ?? generatedId}
      className={className}
      style={isOpen ? style : { ...style, display: 'none' }}
      role="menu"
      hidden={!isOpen}
      aria-hidden={!isOpen}
      {...otherProps}
    >
      {isOpen ? (
        <Scroller className={styles.scroller}>{children}</Scroller>
      ) : null}
    </div>
  );
}

export default MenuContent;
