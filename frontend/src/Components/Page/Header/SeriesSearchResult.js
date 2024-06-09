import PropTypes from 'prop-types';
import React from 'react';
import Label from 'Components/Label';
import { kinds } from 'Helpers/Props';
import SeriesPoster from 'Series/SeriesPoster';
import styles from './SeriesSearchResult.css';

function SeriesSearchResult(props) {
  const {
    match,
    title,
    images,
    alternateTitles,
    tvdbId,
    tvMazeId,
    imdbId,
    tmdbId,
    tags
  } = props;

  let alternateTitle = null;
  let tag = null;

  if (match.key === 'alternateTitles.title') {
    alternateTitle = alternateTitles[match.arrayIndex];
  } else if (match.key === 'tags.label') {
    tag = tags[match.arrayIndex];
  }

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
          alternateTitle ?
            <div className={styles.alternateTitle}>
              {alternateTitle.title}
            </div> :
            null
        }

        {
          match.key === 'tvdbId' && tvdbId ?
            <div className={styles.alternateTitle}>
              TvdbId: {tvdbId}
            </div> :
            null
        }

        {
          match.key === 'tvMazeId' && tvMazeId ?
            <div className={styles.alternateTitle}>
              TvMazeId: {tvMazeId}
            </div> :
            null
        }

        {
          match.key === 'imdbId' && imdbId ?
            <div className={styles.alternateTitle}>
              ImdbId: {imdbId}
            </div> :
            null
        }

        {
          match.key === 'tmdbId' && tmdbId ?
            <div className={styles.alternateTitle}>
              TmdbId: {tmdbId}
            </div> :
            null
        }

        {
          tag ?
            <div className={styles.tagContainer}>
              <Label
                key={tag.id}
                kind={kinds.INFO}
              >
                {tag.label}
              </Label>
            </div> :
            null
        }
      </div>
    </div>
  );
}

SeriesSearchResult.propTypes = {
  title: PropTypes.string.isRequired,
  images: PropTypes.arrayOf(PropTypes.object).isRequired,
  alternateTitles: PropTypes.arrayOf(PropTypes.object).isRequired,
  tvdbId: PropTypes.number,
  tvMazeId: PropTypes.number,
  imdbId: PropTypes.string,
  tmdbId: PropTypes.number,
  tags: PropTypes.arrayOf(PropTypes.object).isRequired,
  match: PropTypes.object.isRequired
};

export default SeriesSearchResult;
