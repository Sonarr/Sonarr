import React from 'react';
import Link, { LinkProps } from 'Components/Link/Link';
import TableRow from './TableRow';
import styles from './TableRowButton.css';

function TableRowButton(props: LinkProps) {
  return <Link className={styles.row} component={TableRow} {...props} />;
}

export default TableRowButton;
