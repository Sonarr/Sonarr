import React from 'react';
import { AlternateTitle } from 'Series/Series';
import styles from './SeriesAlternateTitles.css';

interface SeriesAlternateTitlesProps {
  alternateTitles: AlternateTitle[];
}

function SeriesAlternateTitles({
  alternateTitles,
}: SeriesAlternateTitlesProps) {
  return (
    <ul>
      {alternateTitles.map((alternateTitle) => {
        return (
          <li key={alternateTitle.title} className={styles.alternateTitle}>
            {alternateTitle.title}
            {alternateTitle.comment ? (
              <span className={styles.comment}> {alternateTitle.comment}</span>
            ) : null}
          </li>
        );
      })}
    </ul>
  );
}

export default SeriesAlternateTitles;
