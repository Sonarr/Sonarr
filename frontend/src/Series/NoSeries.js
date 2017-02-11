import React from 'react';
import { kinds } from 'Helpers/Props';
import Button from 'Components/Link/Button';
import styles from './NoSeries.css';

function NoSeries() {
  return (
    <div>
      <div className={styles.message}>
        No series found, to get started you'll want to add a new series or import some existing ones.
      </div>

      <div className={styles.buttonContainer}>
        <Button
          to="/add/import"
          kind={kinds.PRIMARY}
        >
          Import Existing Series
        </Button>
      </div>

      <div className={styles.buttonContainer}>
        <Button
          to="/add/new"
          kind={kinds.PRIMARY}
        >
          Add New Series
        </Button>
      </div>
    </div>
  );
}

export default NoSeries;
