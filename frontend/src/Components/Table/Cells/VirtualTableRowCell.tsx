import React from 'react';
import styles from './VirtualTableRowCell.module.css';

export interface VirtualTableRowCellProps {
  className?: string;
  title?: string;
  role?: string;
  children?: string | React.ReactNode;
}

function VirtualTableRowCell({
  className = styles.cell,
  title,
  role,
  children,
}: VirtualTableRowCellProps) {
  return (
    <div className={className} title={title} role={role}>
      {children}
    </div>
  );
}

export default VirtualTableRowCell;
