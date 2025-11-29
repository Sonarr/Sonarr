import classNames from 'classnames';
import React, { useCallback } from 'react';
import MonitorToggleButton from 'Components/MonitorToggleButton';
import formatSeason from 'Season/formatSeason';
import { Statistics } from 'Series/Series';
import { useToggleSeasonMonitored } from 'Series/useSeries';
import translate from 'Utilities/String/translate';
import styles from './SeasonPassSeason.css';

interface SeasonPassSeasonProps {
  seriesId: number;
  seasonNumber: number;
  monitored: boolean;
  statistics: Statistics;
}

function SeasonPassSeason(props: SeasonPassSeasonProps) {
  const {
    seriesId,
    seasonNumber,
    monitored,
    statistics = {
      episodeFileCount: 0,
      totalEpisodeCount: 0,
      percentOfEpisodes: 0,
    },
  } = props;

  const { episodeFileCount, totalEpisodeCount, percentOfEpisodes } = statistics;

  const { toggleSeasonMonitored, isTogglingSeasonMonitored } =
    useToggleSeasonMonitored(seriesId);
  const onSeasonMonitoredPress = useCallback(() => {
    toggleSeasonMonitored({ seasonNumber, monitored: !monitored });
  }, [seasonNumber, monitored, toggleSeasonMonitored]);

  return (
    <div className={styles.season}>
      <div className={styles.info}>
        <MonitorToggleButton
          monitored={monitored}
          isSaving={isTogglingSeasonMonitored}
          onPress={onSeasonMonitoredPress}
        />

        <span>{formatSeason(seasonNumber, true)}</span>
      </div>

      <div
        className={classNames(
          styles.episodes,
          percentOfEpisodes === 100 && styles.allEpisodes
        )}
        title={translate('SeasonPassEpisodesDownloaded', {
          episodeFileCount,
          totalEpisodeCount,
        })}
      >
        {totalEpisodeCount === 0
          ? '0/0'
          : `${episodeFileCount}/${totalEpisodeCount}`}
      </div>
    </div>
  );
}

export default SeasonPassSeason;
