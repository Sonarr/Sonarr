import classNames from 'classnames';
import React, { useCallback } from 'react';
import { useDispatch } from 'react-redux';
import MonitorToggleButton from 'Components/MonitorToggleButton';
import formatSeason from 'Season/formatSeason';
import { Statistics } from 'Series/Series';
import { toggleSeasonMonitored } from 'Store/Actions/seriesActions';
import translate from 'Utilities/String/translate';
import styles from './SeasonPassSeason.css';

interface SeasonPassSeasonProps {
  seriesId: number;
  seasonNumber: number;
  monitored: boolean;
  statistics: Statistics;
  isSaving: boolean;
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
    isSaving = false,
  } = props;

  const { episodeFileCount, totalEpisodeCount, percentOfEpisodes } = statistics;

  const dispatch = useDispatch();
  const onSeasonMonitoredPress = useCallback(() => {
    dispatch(
      toggleSeasonMonitored({ seriesId, seasonNumber, monitored: !monitored })
    );
  }, [seriesId, seasonNumber, monitored, dispatch]);

  return (
    <div className={styles.season}>
      <div className={styles.info}>
        <MonitorToggleButton
          monitored={monitored}
          isSaving={isSaving}
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
