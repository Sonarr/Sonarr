import React from 'react';
import styles from './PageContentFooter.css';

interface PageContentFooterProps {
  className?: string;
  children: React.ReactNode;
}

function PageContentFooter({
  className = styles.contentFooter,
  children,
}: PageContentFooterProps) {
  return <div className={className}>{children}</div>;
}

export default PageContentFooter;
