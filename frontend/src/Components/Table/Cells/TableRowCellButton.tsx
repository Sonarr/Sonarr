import React, { ReactNode } from 'react';
import Link, { LinkProps } from 'Components/Link/Link';
import TableRowCell from './TableRowCell';
import styles from './TableRowCellButton.css';

interface TableRowCellButtonProps extends LinkProps {
  className?: string;
  children: ReactNode;
}

function TableRowCellButton(props: TableRowCellButtonProps) {
  const { className = styles.cell, children, title, ...otherProps } = props;

  return (
    <TableRowCell className={className}>
      <Link
        className={styles.button}
        title={title}
        aria-label={title}
        {...otherProps}
      >
        {children}
      </Link>
    </TableRowCell>
  );
}

export default TableRowCellButton;
