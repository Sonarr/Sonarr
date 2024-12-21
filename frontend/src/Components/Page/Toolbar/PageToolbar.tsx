import React from 'react';
import styles from './PageToolbar.css';

interface PageToolbarProps {
  className?: string;
  children: React.ReactNode;
}

function PageToolbar({
  className = styles.toolbar,
  children,
}: PageToolbarProps) {
  return <div className={className}>{children}</div>;
}

export default PageToolbar;
