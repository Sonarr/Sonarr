import PropTypes from 'prop-types';
import React from 'react';
import Button from 'Components/Link/Button';
import { kinds } from 'Helpers/Props';
import styles from './NoSeries.css';

function NoSeries(props) {
  const { totalItems } = props;

  if (totalItems > 0) {
    return (
      <div>
        <div className={styles.message}>
          All series are hidden due to the applied filter.
        </div>
      </div>
    );
  }

  return (
    <div>
      <div className={styles.message}>
        No series found, to get started you'll want to import your existing series or add a new series.
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

NoSeries.propTypes = {
  totalItems: PropTypes.number.isRequired
};

export default NoSeries;
