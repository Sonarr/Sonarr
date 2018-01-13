import PropTypes from 'prop-types';
import React from 'react';
import styles from './LoadingIndicator.css';

function LoadingIndicator({ className, size }) {
  const sizeInPx = `${size}px`;
  const width = sizeInPx;
  const height = sizeInPx;

  return (
    <div
      className={className}
      style={{ height }}
    >
      <div
        className={styles.rippleContainer}
        style={{ width, height }}
      >
        <div
          className={styles.ripple}
          style={{ width, height }}
        />

        <div
          className={styles.ripple}
          style={{ width, height }}
        />

        <div
          className={styles.ripple}
          style={{ width, height }}
        />
      </div>
    </div>
  );
}

LoadingIndicator.propTypes = {
  className: PropTypes.string,
  size: PropTypes.number
};

LoadingIndicator.defaultProps = {
  className: styles.loading,
  size: 50
};

export default LoadingIndicator;
