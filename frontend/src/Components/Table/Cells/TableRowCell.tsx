import React, { ComponentPropsWithoutRef } from 'react';
import styles from './TableRowCell.css';

export interface TableRowCellprops extends ComponentPropsWithoutRef<'td'> {}

export default function TableRowCell({
  className = styles.cell,
  ...tdProps
}: TableRowCellprops) {
  return <td className={className} {...tdProps} />;
}
