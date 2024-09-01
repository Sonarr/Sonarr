import React, { ComponentPropsWithoutRef } from 'react';
import styles from './TableRowCell.css';

export interface TableRowCellProps extends ComponentPropsWithoutRef<'td'> {}

export default function TableRowCell({
  className = styles.cell,
  ...tdProps
}: TableRowCellProps) {
  return <td className={className} {...tdProps} />;
}
