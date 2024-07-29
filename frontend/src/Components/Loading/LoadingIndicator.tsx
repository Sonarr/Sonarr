import React from 'react';
import styles from './LoadingIndicator.css';

interface LoadingIndicatorProps {
  className?: string;
  rippleClassName?: string;
  size?: number;
}

function LoadingIndicator({
  className = styles.loading,
  rippleClassName = styles.ripple,
  size = 50,
}: LoadingIndicatorProps) {
  const sizeInPx = `${size}px`;
  const width = sizeInPx;
  const height = sizeInPx;

  return (
    <div className={className} style={{ height }}>
      <div className={styles.rippleContainer} style={{ width, height }}>
        <div className={rippleClassName} style={{ width, height }} />

        <div className={rippleClassName} style={{ width, height }} />

        <div className={rippleClassName} style={{ width, height }} />
      </div>
    </div>
  );
}

export default LoadingIndicator;
