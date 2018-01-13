import PropTypes from 'prop-types';
import React from 'react';
import styles from './SeriesAlternateTitles.css';

function SeriesAlternateTitles({ alternateTitles }) {
  return (
    <ul>
      {
        alternateTitles.map((alternateTitle) => {
          return (
            <li
              key={alternateTitle}
              className={styles.alternateTitle}
            >
              {alternateTitle}
            </li>
          );
        })
      }
    </ul>
  );
}

SeriesAlternateTitles.propTypes = {
  alternateTitles: PropTypes.arrayOf(PropTypes.string).isRequired
};

export default SeriesAlternateTitles;
