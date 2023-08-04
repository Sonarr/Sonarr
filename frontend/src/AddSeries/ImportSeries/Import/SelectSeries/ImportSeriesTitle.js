import PropTypes from 'prop-types';
import React from 'react';
import Label from 'Components/Label';
import { kinds } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import styles from './ImportSeriesTitle.css';

function ImportSeriesTitle(props) {
  const {
    title,
    year,
    network,
    isExistingSeries
  } = props;

  return (
    <div className={styles.titleContainer}>
      <div className={styles.title}>
        {title}
      </div>

      {
        !title.contains(year) &&
        year > 0 ?
          <span className={styles.year}>
            ({year})
          </span> :
          null
      }

      {
        network ?
          <Label>{network}</Label> :
          null
      }

      {
        isExistingSeries ?
          <Label
            kind={kinds.WARNING}
          >
            {translate('Existing')}
          </Label> :
          null
      }
    </div>
  );
}

ImportSeriesTitle.propTypes = {
  title: PropTypes.string.isRequired,
  year: PropTypes.number.isRequired,
  network: PropTypes.string,
  isExistingSeries: PropTypes.bool.isRequired
};

export default ImportSeriesTitle;
