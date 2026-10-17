import React from 'react';
import Popover from 'Components/Tooltip/Popover';
import Episode from 'Episode/Episode';
import EpisodeTitleLink from 'Episode/EpisodeTitleLink';
import Series from 'Series/Series';
import translate from 'Utilities/String/translate';
import styles from './EpisodeTitleCellContent.module.css';

interface EpisodeTitleCellContentProps {
  episodes: Episode[];
  series?: Series;
}

export default function EpisodeTitleCellContent({
  episodes,
  series,
}: EpisodeTitleCellContentProps) {
  if (episodes.length === 0 || !series) {
    return '-';
  }

  if (episodes.length === 1) {
    const episode = episodes[0];

    return (
      <EpisodeTitleLink
        episodeId={episode.id}
        seriesId={series.id}
        episodeTitle={episode.title}
        episodeEntity="episodes"
        showOpenSeriesButton={true}
      />
    );
  }

  return (
    <Popover
      anchor={
        <span className={styles.multiple}>{translate('MultipleEpisodes')}</span>
      }
      title={translate('EpisodeTitles')}
      body={
        <>
          {episodes.map((episode) => {
            return (
              <div key={episode.id} className={styles.row}>
                <div className={styles.episodeNumber}>
                  {episode.episodeNumber}
                </div>

                <EpisodeTitleLink
                  episodeId={episode.id}
                  seriesId={series.id}
                  episodeTitle={episode.title}
                  episodeEntity="episodes"
                  showOpenSeriesButton={true}
                />
              </div>
            );
          })}
        </>
      }
      position="right"
    />
  );
}
