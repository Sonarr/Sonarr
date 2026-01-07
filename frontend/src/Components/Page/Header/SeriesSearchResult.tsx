import React from 'react';
import Label from 'Components/Label';
import { kinds } from 'Helpers/Props';
import SeriesPoster from 'Series/SeriesPoster';
import { Tag } from 'Tags/useTags';
import { SuggestedSeries } from './SeriesSearchInput';
import styles from './SeriesSearchResult.css';

interface Match {
  key: string;
  refIndex: number;
}

interface SeriesSearchResultProps extends SuggestedSeries {
  match: Match;
}

function SeriesSearchResult(props: SeriesSearchResultProps) {
  const {
    match,
    title,
    images,
    alternateTitles,
    tvdbId,
    tvMazeId,
    imdbId,
    tmdbId,
    tags,
  } = props;

  let alternateTitle = null;
  let tag: Tag | null = null;

  if (match.key === 'alternateTitles.title') {
    alternateTitle = alternateTitles[match.refIndex];
  } else if (match.key === 'tags.label') {
    tag = tags[match.refIndex];
  }

  return (
    <div className={styles.result}>
      <SeriesPoster
        className={styles.poster}
        images={images}
        size={250}
        lazy={false}
        overflow={true}
        title={title}
      />

      <div className={styles.titles}>
        <div className={styles.title}>{title}</div>

        {alternateTitle ? (
          <div className={styles.alternateTitle}>{alternateTitle.title}</div>
        ) : null}

        {match.key === 'tvdbId' && tvdbId ? (
          <div className={styles.alternateTitle}>TvdbId: {tvdbId}</div>
        ) : null}

        {match.key === 'tvMazeId' && tvMazeId ? (
          <div className={styles.alternateTitle}>TvMazeId: {tvMazeId}</div>
        ) : null}

        {match.key === 'imdbId' && imdbId ? (
          <div className={styles.alternateTitle}>ImdbId: {imdbId}</div>
        ) : null}

        {match.key === 'tmdbId' && tmdbId ? (
          <div className={styles.alternateTitle}>TmdbId: {tmdbId}</div>
        ) : null}

        {tag ? (
          <div className={styles.tagContainer}>
            <Label key={tag.id} kind={kinds.INFO}>
              {tag.label}
            </Label>
          </div>
        ) : null}
      </div>
    </div>
  );
}

export default SeriesSearchResult;
