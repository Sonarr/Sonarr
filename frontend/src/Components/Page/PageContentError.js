import React from 'react';
import ErrorBoundaryError from 'Components/Error/ErrorBoundaryError';
import PageContentBody from './PageContentBody';
import styles from './PageContentError.css';

function PageContentError(props) {
  return (
    <div className={styles.content}>
      <PageContentBody>
        <ErrorBoundaryError
          {...props}
          message='There was an error loading this page'
        />
      </PageContentBody>
    </div>
  );
}

export default PageContentError;
