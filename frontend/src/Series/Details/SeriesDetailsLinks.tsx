import React, { useMemo } from 'react';
import Label from 'Components/Label';
import ClipboardButton from 'Components/Link/ClipboardButton';
import Link from 'Components/Link/Link';
import { kinds, sizes } from 'Helpers/Props';
import Series from 'Series/Series';
import translate from 'Utilities/String/translate';
import styles from './SeriesDetailsLinks.css';

type SeriesDetailsLinksProps = Pick<
  Series,
  'tvdbId' | 'tvMazeId' | 'imdbId' | 'tmdbId'
>;

interface SeriesDetailsLink {
  externalId: string | number;
  name: string;
  url: string;
}

function SeriesDetailsLinks(props: SeriesDetailsLinksProps) {
  const { tvdbId, tvMazeId, imdbId, tmdbId } = props;

  const links = useMemo(() => {
    const validLinks: SeriesDetailsLink[] = [];

    if (tvdbId) {
      validLinks.push(
        {
          externalId: tvdbId,
          name: 'The TVDB',
          url: `https://www.thetvdb.com/?tab=series&id=${tvdbId}`,
        },
        {
          externalId: tvdbId,
          name: 'Trakt',
          url: `https://trakt.tv/search/tvdb/${tvdbId}?id_type=show`,
        }
      );
    }

    if (tvMazeId) {
      validLinks.push({
        externalId: tvMazeId,
        name: 'TV Maze',
        url: `https://www.tvmaze.com/shows/${tvMazeId}/_`,
      });
    }

    if (imdbId) {
      validLinks.push(
        {
          externalId: imdbId,
          name: 'IMDB',
          url: `https://imdb.com/title/${imdbId}/`,
        },
        {
          externalId: imdbId,
          name: 'MDBList',
          url: `https://mdblist.com/show/${imdbId}`,
        }
      );
    }

    if (tmdbId) {
      validLinks.push({
        externalId: tmdbId,
        name: 'TMDB',
        url: `https://www.themoviedb.org/tv/${tmdbId}`,
      });
    }

    return validLinks;
  }, [tvdbId, tvMazeId, imdbId, tmdbId]);

  return (
    <div className={styles.links}>
      {links.map((link) => (
        <div key={link.name} className={styles.linkBlock}>
          <Link className={styles.link} to={link.url}>
            <Label
              className={styles.linkLabel}
              kind={kinds.INFO}
              size={sizes.LARGE}
            >
              {link.name}
            </Label>
          </Link>

          <ClipboardButton
            value={`${link.externalId}`}
            title={translate('CopyToClipboard')}
            kind={kinds.DEFAULT}
            size={sizes.SMALL}
            label={link.externalId}
          />
        </div>
      ))}
    </div>
  );
}

export default SeriesDetailsLinks;
