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
              key={alternateTitle.title}
              className={styles.alternateTitle}
            >
              {alternateTitle.title}
              {
                alternateTitle.comment &&
                  <span className={styles.comment}> {alternateTitle.comment}</span>
              }
            </li>
          );
        })
      }
    </ul>
  );
}

SeriesAlternateTitles.propTypes = {
  alternateTitles: PropTypes.arrayOf(PropTypes.object).isRequired
};

export default SeriesAlternateTitles;
