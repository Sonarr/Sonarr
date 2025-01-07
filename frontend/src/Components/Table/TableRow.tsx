import React from 'react';
import styles from './TableRow.css';

interface TableRowProps extends React.HTMLAttributes<HTMLTableRowElement> {
  className?: string;
  children?: React.ReactNode;
  overlayContent?: boolean;
}

function TableRow({
  className = styles.row,
  children,
  overlayContent,
  ...otherProps
}: TableRowProps) {
  return (
    <tr className={className} {...otherProps}>
      {children}
    </tr>
  );
}

export default TableRow;
