import React from 'react';
import Link, { LinkProps } from 'Components/Link/Link';
import VirtualTableRow from './VirtualTableRow';
import styles from './VirtualTableRowButton.css';

function VirtualTableRowButton(props: LinkProps) {
  return <Link className={styles.row} component={VirtualTableRow} {...props} />;
}

export default VirtualTableRowButton;
