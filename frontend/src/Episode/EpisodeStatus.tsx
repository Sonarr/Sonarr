import React from 'react';
import { useQueueItemForEpisode } from 'Activity/Queue/Details/QueueDetailsProvider';
import QueueDetails from 'Activity/Queue/QueueDetails';
import Icon from 'Components/Icon';
import ProgressBar from 'Components/ProgressBar';
import useEpisode, { EpisodeEntity } from 'Episode/useEpisode';
import useEpisodeFile from 'EpisodeFile/useEpisodeFile';
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
      <div className={styles.center}>
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
      </div>
    );
  }

  if (grabbed) {
    return (
      <div className={styles.center}>
        <Icon
          name={icons.DOWNLOADING}
          title={translate('EpisodeIsDownloading')}
        />
      </div>
    );
  }

  if (hasEpisodeFile) {
    const quality = episodeFile.quality;
    const isCutoffNotMet = episodeFile.qualityCutoffNotMet;

    return (
      <div className={styles.center}>
        <EpisodeQuality
          quality={quality}
          size={episodeFile.size}
          isCutoffNotMet={isCutoffNotMet}
          title={translate('EpisodeDownloaded')}
        />
      </div>
    );
  }

  if (!airDateUtc) {
    return (
      <div className={styles.center}>
        <Icon name={icons.TBA} title={translate('Tba')} />
      </div>
    );
  }

  if (!monitored) {
    return (
      <div className={styles.center}>
        <Icon
          name={icons.UNMONITORED}
          kind={kinds.DISABLED}
          title={translate('EpisodeIsNotMonitored')}
        />
      </div>
    );
  }

  if (hasAired) {
    return (
      <div className={styles.center}>
        <Icon
          name={icons.MISSING}
          title={translate('EpisodeMissingFromDisk')}
        />
      </div>
    );
  }

  return (
    <div className={styles.center}>
      <Icon name={icons.NOT_AIRED} title={translate('EpisodeHasNotAired')} />
    </div>
  );
}

export default EpisodeStatus;
