import PropTypes from 'prop-types';
import React from 'react';
import Icon from 'Components/Icon';
import Popover from 'Components/Tooltip/Popover';
import { icons, tooltipPositions } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import QueueStatus from './QueueStatus';
import styles from './QueueDetails.css';

function QueueDetails(props) {
  const {
    title,
    size,
    sizeleft,
    status,
    trackedDownloadState,
    trackedDownloadStatus,
    statusMessages,
    errorMessage,
    progressBar
  } = props;

  const progress = (100 - sizeleft / size * 100);
  const isDownloading = status === 'downloading';
  const isPaused = status === 'paused';
  const hasWarning = trackedDownloadStatus === 'warning';
  const hasError = trackedDownloadStatus === 'error';

  if (
    (isDownloading || isPaused) &&
    !hasWarning &&
    !hasError
  ) {
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
        anchor={progressBar}
        title={`${state} - ${progress.toFixed(1)}%`}
        body={
          <div>{title}</div>
        }
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

QueueDetails.propTypes = {
  title: PropTypes.string.isRequired,
  size: PropTypes.number.isRequired,
  sizeleft: PropTypes.number.isRequired,
  estimatedCompletionTime: PropTypes.string,
  status: PropTypes.string.isRequired,
  trackedDownloadState: PropTypes.string.isRequired,
  trackedDownloadStatus: PropTypes.string.isRequired,
  statusMessages: PropTypes.arrayOf(PropTypes.object),
  errorMessage: PropTypes.string,
  progressBar: PropTypes.node.isRequired
};

QueueDetails.defaultProps = {
  trackedDownloadStatus: 'ok',
  trackedDownloadState: 'downloading'
};

export default QueueDetails;
