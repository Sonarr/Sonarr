import React from 'react';
import ProgressBar from 'Components/ProgressBar';
import { sizes } from 'Helpers/Props';
import getProgressBarKind from 'Utilities/Series/getProgressBarKind';
import styles from './SeriesIndexProgressBar.css';

interface SeriesIndexProgressBarProps {
  monitored: boolean;
  status: string;
  episodeCount: number;
  episodeFileCount: number;
  totalEpisodeCount: number;
  posterWidth: number;
  detailedProgressBar: boolean;
}

function SeriesIndexProgressBar(props: SeriesIndexProgressBarProps) {
  const {
    monitored,
    status,
    episodeCount,
    episodeFileCount,
    totalEpisodeCount,
    posterWidth,
    detailedProgressBar,
  } = props;

  const progress = episodeCount ? (episodeFileCount / episodeCount) * 100 : 100;
  const text = `${episodeFileCount} / ${episodeCount}`;

  return (
    <ProgressBar
      className={styles.progressBar}
      containerClassName={styles.progress}
      progress={progress}
      kind={getProgressBarKind(status, monitored, progress)}
      size={detailedProgressBar ? sizes.MEDIUM : sizes.SMALL}
      showText={detailedProgressBar}
      text={text}
      title={`${episodeFileCount} / ${episodeCount} (Total: ${totalEpisodeCount})`}
      width={posterWidth}
    />
  );
}

export default SeriesIndexProgressBar;
