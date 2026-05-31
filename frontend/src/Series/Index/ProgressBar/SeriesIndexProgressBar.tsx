import React, { useMemo } from 'react';
import { useQueueDetailsForSeries } from 'Activity/Queue/Details/QueueDetailsProvider';
import ProgressBar from 'Components/ProgressBar';
import { sizes } from 'Helpers/Props';
import { SeriesStatus } from 'Series/Series';
import getProgressBarKind from 'Utilities/Series/getProgressBarKind';
import translate from 'Utilities/String/translate';
import styles from './SeriesIndexProgressBar.css';

interface SeriesIndexProgressBarProps {
  seriesId: number;
  seasonNumber?: number;
  monitored: boolean;
  status: SeriesStatus;
  episodeCount: number;
  episodeFileCount: number;
  totalEpisodeCount: number;
  width: number;
  detailedProgressBar: boolean;
  isStandalone: boolean;
}

function SeriesIndexProgressBar(props: SeriesIndexProgressBarProps) {
  const {
    seriesId,
    seasonNumber,
    monitored,
    status,
    episodeCount,
    episodeFileCount,
    totalEpisodeCount,
    width,
    detailedProgressBar,
    isStandalone,
  } = props;

  const queueDetails = useQueueDetailsForSeries(seriesId, seasonNumber);

  const containerClassName = useMemo(() => {
    if (isStandalone) {
      return undefined;
    }

    if (detailedProgressBar) return styles.progressDetailed;
    return styles.progress;
  }, [isStandalone, detailedProgressBar]);

  const newDownloads = queueDetails.count - queueDetails.episodesWithFiles;
  const progress = episodeCount ? (episodeFileCount / episodeCount) * 100 : 100;
  const text = newDownloads
    ? `${episodeFileCount} + ${newDownloads} / ${episodeCount}`
    : `${episodeFileCount} / ${episodeCount}`;

  return (
    <ProgressBar
      className={
        detailedProgressBar ? styles.progressBarDetailed : styles.progressBar
      }
      containerClassName={containerClassName}
      progress={progress}
      kind={getProgressBarKind(
        status,
        monitored,
        progress,
        queueDetails.count > 0
      )}
      size={detailedProgressBar ? sizes.MEDIUM : sizes.SMALL}
      showText={detailedProgressBar}
      text={text}
      title={translate('SeriesProgressBarText', {
        episodeFileCount,
        episodeCount,
        totalEpisodeCount,
        downloadingCount: queueDetails.count,
      })}
      width={width}
    />
  );
}

export default SeriesIndexProgressBar;
