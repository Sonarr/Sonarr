import _ from 'lodash';
import PropTypes from 'prop-types';
import React from 'react';
import SeriesPoster from 'Series/SeriesPoster';
import styles from './SeriesSearchResult.css';

function getMatchingAlternateTile(alternateTitles, query) {
  return _.first(alternateTitles, (alternateTitle) => {
    return alternateTitle.title.toLowerCase().contains(query.toLowerCase());
  });
}

function SeriesSearchResult(props) {
  const {
    query,
    title,
    alternateTitles,
    images
  } = props;

  const index = title.toLowerCase().indexOf(query.toLowerCase());
  const alternateTitle = index === -1 ?
    getMatchingAlternateTile(alternateTitles, query) :
    null;

  return (
    <div className={styles.result}>
      <SeriesPoster
        className={styles.poster}
        images={images}
        size={250}
        lazy={false}
        overflow={true}
      />

      <div className={styles.titles}>
        <div className={styles.title}>
          {title}
        </div>

        {
          !!alternateTitle &&
            <div className={styles.alternateTitle}>
              {alternateTitle.title}
            </div>
        }
      </div>
    </div>
  );
}

SeriesSearchResult.propTypes = {
  query: PropTypes.string.isRequired,
  title: PropTypes.string.isRequired,
  alternateTitles: PropTypes.arrayOf(PropTypes.object).isRequired,
  images: PropTypes.arrayOf(PropTypes.object).isRequired
};

export default SeriesSearchResult;
