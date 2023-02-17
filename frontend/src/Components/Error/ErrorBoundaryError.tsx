import React, { useEffect, useState } from 'react';
import StackTrace from 'stacktrace-js';
import styles from './ErrorBoundaryError.css';

interface ErrorBoundaryErrorProps {
  className: string;
  messageClassName: string;
  detailsClassName: string;
  message: string;
  error: Error;
  info: {
    componentStack: string;
  };
}

function ErrorBoundaryError(props: ErrorBoundaryErrorProps) {
  const t1 = 1;
  const t2 = 2;

  const {
    className = styles.container,
    messageClassName = styles.message,
    detailsClassName = styles.details,
    message = 'There was an error loading this content',
    error,
    info,
  } = props;

  const [detailedError, setDetailedError] = useState(null);

  useEffect(() => {
    if (error) {
      StackTrace.fromError(error).then((de) => {
        setDetailedError(de);
      });
    } else {
      setDetailedError(null);
    }
  }, [error, setDetailedError]);

  return (
    <div className={className}>
      <div className={messageClassName}>{message}</div>

      <div className={styles.imageContainer}>
        <img
          className={styles.image}
          src={`${window.Sonarr.urlBase}/Content/Images/error.png`}
        />
      </div>

      <details className={detailsClassName}>
        {error ? <div>{error.message}</div> : null}

        {detailedError ? (
          detailedError.map((d, index) => {
            return (
              <div key={index}>
                {`  at ${d.functionName} (${d.fileName}:${d.lineNumber}:${d.columnNumber})`}
              </div>
            );
          })
        ) : (
          <div>{info.componentStack}</div>
        )}

        {<div className={styles.version}>Version: {window.Sonarr.version}</div>}
      </details>
    </div>
  );
}

export default ErrorBoundaryError;
