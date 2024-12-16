import React from 'react';
import ErrorBoundaryError, {
  ErrorBoundaryErrorProps,
} from 'Components/Error/ErrorBoundaryError';
import translate from 'Utilities/String/translate';
import PageContentBody from './PageContentBody';
import styles from './PageContentError.css';

function PageContentError(props: ErrorBoundaryErrorProps) {
  return (
    <div className={styles.content}>
      <PageContentBody>
        <ErrorBoundaryError
          {...props}
          message={translate('ErrorLoadingPage')}
        />
      </PageContentBody>
    </div>
  );
}

export default PageContentError;
