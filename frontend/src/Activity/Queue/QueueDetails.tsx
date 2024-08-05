import React from 'react';
import Icon from 'Components/Icon';
import Popover from 'Components/Tooltip/Popover';
import { icons, tooltipPositions } from 'Helpers/Props';
import {
  QueueTrackedDownloadState,
  QueueTrackedDownloadStatus,
  StatusMessage,
} from 'typings/Queue';
import translate from 'Utilities/String/translate';
import QueueStatus from './QueueStatus';
import styles from './QueueDetails.css';

interface QueueDetailsProps {
  title: string;
  size: number;
  sizeleft: number;
  estimatedCompletionTime?: string;
  status: string;
  trackedDownloadState?: QueueTrackedDownloadState;
  trackedDownloadStatus?: QueueTrackedDownloadStatus;
  statusMessages?: StatusMessage[];
  errorMessage?: string;
  progressBar: React.ReactNode;
}

function QueueDetails(props: QueueDetailsProps) {
  const {
    title,
    size,
    sizeleft,
    status,
    trackedDownloadState = 'downloading',
    trackedDownloadStatus = 'ok',
    statusMessages,
    errorMessage,
    progressBar,
  } = props;

  const progress = 100 - (sizeleft / size) * 100;
  const isDownloading = status === 'downloading';
  const isPaused = status === 'paused';
  const hasWarning = trackedDownloadStatus === 'warning';
  const hasError = trackedDownloadStatus === 'error';

  if ((isDownloading || isPaused) && !hasWarning && !hasError) {
    const state = isPaused ? translate('Paused') : translate('Downloading');

    if (progress < 5) {
      return (
        <Icon
          name={icons.DOWNLOADING}
          title={`${state} - ${progress.toFixed(1)}% ${title}`}
        />
      );
    }

    return (
      <Popover
        className={styles.progressBarContainer}
        anchor={progressBar!}
        title={`${state} - ${progress.toFixed(1)}%`}
        body={<div>{title}</div>}
        position={tooltipPositions.LEFT}
      />
    );
  }

  return (
    <QueueStatus
      sourceTitle={title}
      status={status}
      trackedDownloadStatus={trackedDownloadStatus}
      trackedDownloadState={trackedDownloadState}
      statusMessages={statusMessages}
      errorMessage={errorMessage}
      position={tooltipPositions.LEFT}
    />
  );
}

export default QueueDetails;
