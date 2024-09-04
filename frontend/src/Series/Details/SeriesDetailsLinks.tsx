import React from 'react';
import Label from 'Components/Label';
import Link from 'Components/Link/Link';
import { kinds, sizes } from 'Helpers/Props';
import Series from 'Series/Series';
import styles from './SeriesDetailsLinks.css';

type SeriesDetailsLinksProps = Pick<
  Series,
  'tvdbId' | 'tvMazeId' | 'imdbId' | 'tmdbId'
>;

function SeriesDetailsLinks(props: SeriesDetailsLinksProps) {
  const { tvdbId, tvMazeId, imdbId, tmdbId } = props;

  return (
    <div className={styles.links}>
      <Link
        className={styles.link}
        to={`https://www.thetvdb.com/?tab=series&id=${tvdbId}`}
      >
        <Label
          className={styles.linkLabel}
          kind={kinds.INFO}
          size={sizes.LARGE}
        >
          The TVDB
        </Label>
      </Link>

      <Link
        className={styles.link}
        to={`https://trakt.tv/search/tvdb/${tvdbId}?id_type=show`}
      >
        <Label
          className={styles.linkLabel}
          kind={kinds.INFO}
          size={sizes.LARGE}
        >
          Trakt
        </Label>
      </Link>

      {tvMazeId ? (
        <Link
          className={styles.link}
          to={`https://www.tvmaze.com/shows/${tvMazeId}/_`}
        >
          <Label
            className={styles.linkLabel}
            kind={kinds.INFO}
            size={sizes.LARGE}
          >
            TV Maze
          </Label>
        </Link>
      ) : null}

      {imdbId ? (
        <>
          <Link
            className={styles.link}
            to={`https://imdb.com/title/${imdbId}/`}
          >
            <Label
              className={styles.linkLabel}
              kind={kinds.INFO}
              size={sizes.LARGE}
            >
              IMDB
            </Label>
          </Link>

          <Link
            className={styles.link}
            to={`http://mdblist.com/show/${imdbId}`}
          >
            <Label
              className={styles.linkLabel}
              kind={kinds.INFO}
              size={sizes.LARGE}
            >
              MDBList
            </Label>
          </Link>
        </>
      ) : null}

      {tmdbId ? (
        <Link
          className={styles.link}
          to={`https://www.themoviedb.org/tv/${tmdbId}`}
        >
          <Label
            className={styles.linkLabel}
            kind={kinds.INFO}
            size={sizes.LARGE}
          >
            TMDB
          </Label>
        </Link>
      ) : null}
    </div>
  );
}

export default SeriesDetailsLinks;
