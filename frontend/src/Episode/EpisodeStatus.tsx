import React from 'react';
import { useQueueItemForEpisode } from 'Activity/Queue/Details/QueueDetailsProvider';
import QueueDetails from 'Activity/Queue/QueueDetails';
import Icon from 'Components/Icon';
import ProgressBar from 'Components/ProgressBar';
import StatusIndicator from 'Components/StatusIndicator';
import useEpisode, { EpisodeEntity } from 'Episode/useEpisode';
import { useEpisodeFile } from 'EpisodeFile/EpisodeFileProvider';
import { icons, kinds, sizes } from 'Helpers/Props';
import isBefore from 'Utilities/Date/isBefore';
import translate from 'Utilities/String/translate';
import EpisodeQuality from './EpisodeQuality';
import styles from './EpisodeStatus.css';

interface EpisodeStatusProps {
  episodeId: number;
  episodeEntity?: EpisodeEntity;
  episodeFileId: number | undefined;
}

function EpisodeStatus({
  episodeId,
  episodeEntity = 'episodes',
  episodeFileId,
}: EpisodeStatusProps) {
  const episode = useEpisode(episodeId, episodeEntity);
  const queueItem = useQueueItemForEpisode(episodeId);
  const episodeFile = useEpisodeFile(episodeFileId);

  const { airDateUtc, grabbed, monitored } = episode || {};
  const hasEpisodeFile = !!episodeFile;
  const isQueued = !!queueItem;
  const hasAired = isBefore(airDateUtc);

  if (!episode) {
    return null;
  }

  if (isQueued) {
    const { sizeLeft, size } = queueItem;

    const progress = size ? 100 - (sizeLeft / size) * 100 : 0;

    return (
      <StatusIndicator
        className={styles.center}
        label={translate('EpisodeIsDownloading')}
      >
        <QueueDetails
          {...queueItem}
          progressBar={
            <ProgressBar
              progress={progress}
              kind={kinds.PURPLE}
              size={sizes.MEDIUM}
            />
          }
        />
      </StatusIndicator>
    );
  }

  if (grabbed) {
    const label = translate('EpisodeIsDownloading');

    return (
      <StatusIndicator className={styles.center} label={label} title={label}>
        <Icon name={icons.DOWNLOADING} />
      </StatusIndicator>
    );
  }

  if (hasEpisodeFile) {
    const quality = episodeFile.quality;
    const isCutoffNotMet = episodeFile.qualityCutoffNotMet;
    const label = translate('EpisodeDownloaded');

    return (
      <StatusIndicator className={styles.center} label={label}>
        <EpisodeQuality
          quality={quality}
          size={episodeFile.size}
          isCutoffNotMet={isCutoffNotMet}
          title={label}
        />
      </StatusIndicator>
    );
  }

  if (!airDateUtc) {
    const label = translate('Tba');

    return (
      <StatusIndicator className={styles.center} label={label} title={label}>
        <Icon name={icons.TBA} />
      </StatusIndicator>
    );
  }

  if (!monitored) {
    const label = translate('EpisodeIsNotMonitored');

    return (
      <StatusIndicator className={styles.center} label={label} title={label}>
        <Icon name={icons.UNMONITORED} kind={kinds.DISABLED} />
      </StatusIndicator>
    );
  }

  if (hasAired) {
    const label = translate('EpisodeMissingFromDisk');

    return (
      <StatusIndicator className={styles.center} label={label} title={label}>
        <Icon name={icons.MISSING} />
      </StatusIndicator>
    );
  }

  const label = translate('EpisodeHasNotAired');

  return (
    <StatusIndicator className={styles.center} label={label} title={label}>
      <Icon name={icons.NOT_AIRED} />
    </StatusIndicator>
  );
}

export default EpisodeStatus;
