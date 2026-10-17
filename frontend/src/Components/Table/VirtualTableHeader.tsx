import React from 'react';
import styles from './VirtualTableHeader.module.css';

interface VirtualTableHeaderProps {
  children?: React.ReactNode;
}

function VirtualTableHeader({ children }: VirtualTableHeaderProps) {
  return <div className={styles.header}>{children}</div>;
}

export default VirtualTableHeader;
