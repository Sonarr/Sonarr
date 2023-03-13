import React from 'react';
import { useSelector } from 'react-redux';
import ProgressBar from 'Components/ProgressBar';
import { sizes } from 'Helpers/Props';
import createSeriesQueueItemsDetailsSelector, {
  SeriesQueueDetails,
} from 'Series/Index/createSeriesQueueDetailsSelector';
import getProgressBarKind from 'Utilities/Series/getProgressBarKind';
import styles from './SeriesIndexProgressBar.css';

interface SeriesIndexProgressBarProps {
  seriesId: number;
  seasonNumber?: number;
  monitored: boolean;
  status: string;
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

  const queueDetails: SeriesQueueDetails = useSelector(
    createSeriesQueueItemsDetailsSelector(seriesId, seasonNumber)
  );

  const newDownloads = queueDetails.count - queueDetails.episodesWithFiles;
  const progress = episodeCount ? (episodeFileCount / episodeCount) * 100 : 100;
  const text = newDownloads
    ? `${episodeFileCount} + ${newDownloads} / ${episodeCount}`
    : `${episodeFileCount} / ${episodeCount}`;

  return (
    <ProgressBar
      className={styles.progressBar}
      containerClassName={isStandalone ? undefined : styles.progress}
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
      title={`${episodeFileCount} / ${episodeCount} (Total: ${totalEpisodeCount}, Downloading: ${queueDetails.count})`}
      width={width}
    />
  );
}

export default SeriesIndexProgressBar;
