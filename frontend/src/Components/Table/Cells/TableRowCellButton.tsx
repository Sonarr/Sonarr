import React, { ReactNode } from 'react';
import Link, { LinkProps } from 'Components/Link/Link';
import TableRowCell from './TableRowCell';
import styles from './TableRowCellButton.css';

interface TableRowCellButtonProps extends LinkProps {
  className?: string;
  children: ReactNode;
}

function TableRowCellButton(props: TableRowCellButtonProps) {
  const { className = styles.cell, ...otherProps } = props;

  return (
    <Link className={className} component={TableRowCell} {...otherProps} />
  );
}

export default TableRowCellButton;
