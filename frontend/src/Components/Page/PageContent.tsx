import React from 'react';
import DocumentTitle from 'react-document-title';
import ErrorBoundary from 'Components/Error/ErrorBoundary';
import PageContentError from './PageContentError';
import styles from './PageContent.css';

interface PageContentProps {
  className?: string;
  title?: string;
  children: React.ReactNode;
}

function PageContent({
  className = styles.content,
  title,
  children,
}: PageContentProps) {
  return (
    <ErrorBoundary errorComponent={PageContentError}>
      <DocumentTitle
        title={
          title
            ? `${title} - ${window.Sonarr.instanceName}`
            : window.Sonarr.instanceName
        }
      >
        <div className={className}>{children}</div>
      </DocumentTitle>
    </ErrorBoundary>
  );
}

export default PageContent;
