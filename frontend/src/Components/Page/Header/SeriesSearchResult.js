import PropTypes from 'prop-types';
import React from 'react';
import { kinds } from 'Helpers/Props';
import Label from 'Components/Label';
import SeriesPoster from 'Series/SeriesPoster';
import styles from './SeriesSearchResult.css';

function findMatchingAlternateTitle(alternateTitles, cleanQuery) {
  return alternateTitles.find((alternateTitle) => {
    return alternateTitle.cleanTitle.contains(cleanQuery);
  });
}

function getMatchingTag(tags, cleanQuery) {
  return tags.find((tag) => {
    return tag.cleanLabel.contains(cleanQuery);
  });
}

function SeriesSearchResult(props) {
  const {
    cleanQuery,
    title,
    cleanTitle,
    images,
    alternateTitles,
    tags
  } = props;

  const titleContains = cleanTitle.contains(cleanQuery);
  let alternateTitle = null;
  let tag = null;

  if (!titleContains) {
    alternateTitle = findMatchingAlternateTitle(alternateTitles, cleanQuery);
  }

  if (!titleContains && !alternateTitle) {
    tag = getMatchingTag(tags, cleanQuery);
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
          !!alternateTitle &&
            <div className={styles.alternateTitle}>
              {alternateTitle.title}
            </div>
        }

        {
          !!tag &&
            <div className={styles.tagContainer}>
              <Label
                key={tag.id}
                kind={kinds.INFO}
              >
                {tag.label}
              </Label>
            </div>
        }
      </div>
    </div>
  );
}

SeriesSearchResult.propTypes = {
  cleanQuery: PropTypes.string.isRequired,
  title: PropTypes.string.isRequired,
  cleanTitle: PropTypes.string.isRequired,
  images: PropTypes.arrayOf(PropTypes.object).isRequired,
  alternateTitles: PropTypes.arrayOf(PropTypes.object).isRequired,
  tags: PropTypes.arrayOf(PropTypes.object).isRequired
};

export default SeriesSearchResult;
