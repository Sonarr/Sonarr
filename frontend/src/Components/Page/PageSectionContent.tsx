import React from 'react';
import { Error } from 'App/State/AppSectionState';
import Alert from 'Components/Alert';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import { kinds } from 'Helpers/Props';
import { ApiError } from 'Utilities/Fetch/fetchJson';
import styles from './PageSectionContent.css';

interface PageSectionContentProps {
  isFetching: boolean;
  isPopulated: boolean;
  error?: Error | ApiError | null;
  errorMessage: string;
  children: React.ReactNode;
}

function PageSectionContent({
  isFetching,
  isPopulated,
  error,
  errorMessage,
  children,
}: PageSectionContentProps) {
  if (isFetching && !isPopulated) {
    return <LoadingIndicator />;
  }

  if (!isFetching && !!error) {
    return <Alert kind={kinds.DANGER}>{errorMessage}</Alert>;
  }

  if (isPopulated && !error) {
    return <div className={styles.content}>{children}</div>;
  }

  return null;
}

export default PageSectionContent;
