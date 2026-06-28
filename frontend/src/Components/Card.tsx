import React from 'react';
import Link, { LinkProps } from 'Components/Link/Link';
import styles from './Card.css';

interface CardProps
  extends Pick<LinkProps, 'aria-label' | 'onPress' | 'title'> {
  // TODO: Consider using different properties for classname depending if it's overlaying content or not
  className?: string;
  overlayClassName?: string;
  overlayContent?: boolean;
  children: React.ReactNode;
}

function Card(props: CardProps) {
  const {
    className = styles.card,
    overlayClassName = styles.overlay,
    overlayContent = false,
    children,
    'aria-label': ariaLabel,
    onPress,
    title,
  } = props;

  if (overlayContent) {
    return (
      <div className={className}>
        <Link
          className={styles.underlay}
          aria-label={ariaLabel}
          title={title}
          onPress={onPress}
        />

        <div className={overlayClassName}>{children}</div>
      </div>
    );
  }

  return (
    <Link
      className={className}
      aria-label={ariaLabel}
      title={title}
      onPress={onPress}
    >
      {children}
    </Link>
  );
}

export default Card;
