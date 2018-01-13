import React from 'react';
import ErrorBoundaryError from 'Components/Error/ErrorBoundaryError';
import PageContentBodyConnector from './PageContentBodyConnector';
import styles from './PageContentError.css';

function PageContentError(props) {
  return (
    <div className={styles.content}>
      <PageContentBodyConnector>
        <ErrorBoundaryError
          {...props}
          message='There was an error loading this page'
        />
      </PageContentBodyConnector>
    </div>
  );
}

export default PageContentError;
