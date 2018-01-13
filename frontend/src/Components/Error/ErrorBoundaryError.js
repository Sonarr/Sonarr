import PropTypes from 'prop-types';
import React from 'react';
import styles from './ErrorBoundaryError.css';

function ErrorBoundaryError(props) {
  const {
    className,
    messageClassName,
    detailsClassName,
    message,
    error,
    info
  } = props;

  return (
    <div className={className}>
      <div className={messageClassName}>
        {message}
      </div>

      <div className={styles.imageContainer}>
        <img
          className={styles.image}
          src={`${window.Sonarr.urlBase}/Content/Images/error.png`}
        />
      </div>

      <details className={detailsClassName}>
        {
          error &&
            <div>
              {error.toString()}
            </div>
        }

        <div className={styles.info}>
          {info.componentStack}
        </div>
      </details>
    </div>
  );
}

ErrorBoundaryError.propTypes = {
  className: PropTypes.string.isRequired,
  messageClassName: PropTypes.string.isRequired,
  detailsClassName: PropTypes.string.isRequired,
  message: PropTypes.string.isRequired,
  error: PropTypes.object.isRequired,
  info: PropTypes.object.isRequired
};

ErrorBoundaryError.defaultProps = {
  className: styles.container,
  messageClassName: styles.message,
  detailsClassName: styles.details,
  message: 'There was an error loading this content'
};

export default ErrorBoundaryError;
