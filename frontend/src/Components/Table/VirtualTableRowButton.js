import React from 'react';
import Link from 'Components/Link/Link';
import VirtualTableRow from './VirtualTableRow';
import styles from './VirtualTableRowButton.css';

function VirtualTableRowButton(props) {
  return (
    <Link
      className={styles.row}
      component={VirtualTableRow}
      {...props}
    />
  );
}

export default VirtualTableRowButton;
