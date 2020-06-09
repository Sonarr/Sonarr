import PropTypes from 'prop-types';
import React from 'react';
import { kinds } from 'Helpers/Props';
import Label from 'Components/Label';
import SeriesPoster from 'Series/SeriesPoster';
import styles from './SeriesSearchResult.css';

function SeriesSearchResult(props) {
  const {
    match,
    title,
    images,
    alternateTitles,
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
  tags: PropTypes.arrayOf(PropTypes.object).isRequired,
  match: PropTypes.object.isRequired
};

export default SeriesSearchResult;
