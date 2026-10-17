import moment from 'moment-timezone';
import React from 'react';
import {
  useIsQueueDetailsLoaded,
  useQueueItemForEpisode,
} from 'Activity/Queue/Details/QueueDetailsProvider';
import getQueueStatus from 'Activity/Queue/getQueueStatus';
import QueueStatusMessages from 'Activity/Queue/QueueStatusMessages';
import getStatusStyle from 'Calendar/getStatusStyle';
import CircularProgressBar from 'Components/CircularProgressBar';
import Icon from 'Components/Icon';
import Label from 'Components/Label';
import Popover from 'Components/Tooltip/Popover';
import Episode from 'Episode/Episode';
import { sizes, tooltipPositions } from 'Helpers/Props';
import { CalendarStatus } from 'typings/Calendar';
import Queue from 'typings/Queue';
import translate from 'Utilities/String/translate';
import styles from './EpisodeStatusLabel.module.css';

interface EpisodeStatusLabelProps {
  episode: Episode;
  hasFile: boolean;
  seriesMonitored: boolean;
  seriesRuntime: number;
}

function getStatus(
  hasFile: boolean,
  isDownloading: boolean,
  airDateUtc: string | undefined,
  runtime: number,
  monitored: boolean
): CalendarStatus {
  return getStatusStyle(
    hasFile,
    isDownloading,
    airDateUtc ? moment(airDateUtc) : undefined,
    airDateUtc ? moment(airDateUtc).add(runtime, 'minutes') : undefined,
    monitored
  );
}

function getStatusText(status: CalendarStatus, airDateUtc?: string) {
  switch (status) {
    case 'downloaded':
      return translate('Downloaded');
    case 'downloading':
      return translate('Downloading');
    case 'onAir':
      return translate('OnAir');
    case 'missing':
      return translate('Missing');
    default:
      return airDateUtc ? translate('Unaired') : translate('Tba');
  }
}

function QueueLabel({
  title,
  size,
  sizeLeft,
  status,
  trackedDownloadStatus,
  trackedDownloadState,
  statusMessages = [],
  errorMessage,
}: Queue) {
  const {
    iconName,
    title: statusTitle,
    hasWarning,
    hasError,
  } = getQueueStatus({
    sourceTitle: title,
    status,
    trackedDownloadStatus,
    trackedDownloadState,
    errorMessage,
  });

  const showProgress =
    (status === 'downloading' || status === 'paused') &&
    !hasWarning &&
    !hasError &&
    size > 0;

  const progress = showProgress
    ? Math.min(100, Math.max(0, Math.floor(100 - (sizeLeft / size) * 100)))
    : null;

  return (
    <Popover
      anchor={
        <Label className={styles.downloading} size={sizes.MEDIUM} dot={false}>
          {progress == null ? (
            <Icon name={iconName} size={12} />
          ) : (
            <CircularProgressBar
              containerClassName={styles.progress}
              progress={progress}
              size={12}
              strokeWidth={2}
              strokeColor="var(--labelKindColor)"
            />
          )}

          {progress == null ? statusTitle : `${statusTitle} ${progress}%`}
        </Label>
      }
      title={statusTitle}
      body={
        hasWarning || hasError ? (
          <QueueStatusMessages statusMessages={statusMessages} />
        ) : (
          title
        )
      }
      position={tooltipPositions.BOTTOM}
    />
  );
}

function EpisodeStatusLabel({
  episode,
  hasFile,
  seriesMonitored,
  seriesRuntime,
}: EpisodeStatusLabelProps) {
  const queueItemForEpisode = useQueueItemForEpisode(episode.id);
  const isQueueLoaded = useIsQueueDetailsLoaded();
  const { airDateUtc, grabbed, runtime: episodeRuntime } = episode;

  const queueItem =
    queueItemForEpisode?.trackedDownloadState === 'imported'
      ? undefined
      : queueItemForEpisode;

  if (grabbed && !hasFile && !isQueueLoaded) {
    return null;
  }

  const monitored = episode.monitored && seriesMonitored;
  const runtime = episodeRuntime || seriesRuntime;
  const isDownloading = !!grabbed && !queueItem;

  const status = getStatus(
    hasFile,
    isDownloading,
    airDateUtc,
    runtime,
    monitored
  );

  const textStatus =
    status === 'unmonitored'
      ? getStatus(hasFile, isDownloading, airDateUtc, runtime, true)
      : status;

  return (
    <>
      {hasFile || !queueItem ? (
        <Label className={styles[status]} size={sizes.MEDIUM}>
          {getStatusText(textStatus, airDateUtc)}
        </Label>
      ) : null}

      {queueItem ? <QueueLabel {...queueItem} /> : null}
    </>
  );
}

export default EpisodeStatusLabel;
