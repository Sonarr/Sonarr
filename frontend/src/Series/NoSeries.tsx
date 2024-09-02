import React from 'react';
import Button from 'Components/Link/Button';
import { kinds } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import styles from './NoSeries.css';

interface NoSeriesProps {
  totalItems: number;
}

function NoSeries(props: NoSeriesProps) {
  const { totalItems } = props;

  if (totalItems > 0) {
    return (
      <div>
        <div className={styles.message}>
          {translate('AllSeriesAreHiddenByTheAppliedFilter')}
        </div>
      </div>
    );
  }

  return (
    <div>
      <div className={styles.message}>
        {translate('NoSeriesFoundImportOrAdd')}
      </div>

      <div className={styles.buttonContainer}>
        <Button to="/add/import" kind={kinds.PRIMARY}>
          {translate('ImportExistingSeries')}
        </Button>
      </div>

      <div className={styles.buttonContainer}>
        <Button to="/add/new" kind={kinds.PRIMARY}>
          {translate('AddNewSeries')}
        </Button>
      </div>
    </div>
  );
}

export default NoSeries;
