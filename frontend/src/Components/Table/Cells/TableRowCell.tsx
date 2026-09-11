import React, { ComponentPropsWithoutRef } from 'react';
import styles from './TableRowCell.module.css';

export type TableRowCellProps = ComponentPropsWithoutRef<'td'>;

export default function TableRowCell({
  className = styles.cell,
  ...tdProps
}: TableRowCellProps) {
  return <td className={className} {...tdProps} />;
}
