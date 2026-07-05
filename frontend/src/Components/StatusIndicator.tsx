import classNames from 'classnames';
import React, { ComponentProps, ReactNode } from 'react';
import styles from './StatusIndicator.css';

interface StatusIndicatorProps extends ComponentProps<'span'> {
  label: string;
  children: ReactNode;
}

export default function StatusIndicator({
  className,
  label,
  children,
  ...otherProps
}: StatusIndicatorProps) {
  return (
    <span className={classNames(styles.status, className)} {...otherProps}>
      <span className={styles.label}>{label}</span>
      {children}
    </span>
  );
}
