import React from 'react';
import styles from './VirtualTableHeader.module.css';

interface VirtualTableHeaderProps {
  role?: string;
  children?: React.ReactNode;
}

function VirtualTableHeader({ role, children }: VirtualTableHeaderProps) {
  return (
    <div className={styles.header} role={role}>
      {children}
    </div>
  );
}

export default VirtualTableHeader;
