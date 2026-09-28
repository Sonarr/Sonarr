import React from 'react';
import styles from './VirtualTableRowCell.module.css';

export interface VirtualTableRowCellProps {
  className?: string;
  children?: string | React.ReactNode;
}

function VirtualTableRowCell({
  className = styles.cell,
  children,
}: VirtualTableRowCellProps) {
  return <div className={className}>{children}</div>;
}

export default VirtualTableRowCell;
