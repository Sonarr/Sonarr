import React, { useEffect } from 'react';
import ErrorBoundary from 'Components/Error/ErrorBoundary';
import PageContentError from './PageContentError';
import styles from './PageContent.module.css';

interface PageContentProps {
  className?: string;
  title: string;
  children: React.ReactNode;
}

function PageContent({
  className = styles.content,
  title,
  children,
}: PageContentProps) {
  useEffect(() => {
    document.title = title
      ? `${title} - ${window.Sonarr.instanceName}`
      : window.Sonarr.instanceName;

    return () => {
      document.title = window.Sonarr.instanceName;
    };
  }, [title]);

  return (
    <ErrorBoundary errorComponent={PageContentError}>
      <main className={className} aria-label={title}>
        {children}
      </main>
    </ErrorBoundary>
  );
}

export default PageContent;
