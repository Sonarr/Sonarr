import React from 'react';
import styles from './VirtualTableRow.css';

interface VirtualTableRowProps extends React.HTMLAttributes<HTMLDivElement> {
  className: string;
  style: object;
  children?: React.ReactNode;
}

function VirtualTableRow({
  className = styles.row,
  children,
  style,
  ...otherProps
}: VirtualTableRowProps) {
  return (
    <div className={className} style={style} {...otherProps}>
      {children}
    </div>
  );
}

export default VirtualTableRow;
