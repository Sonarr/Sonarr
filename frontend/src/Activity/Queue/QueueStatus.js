import PropTypes from 'prop-types';
import React from 'react';
import Icon from 'Components/Icon';
import Popover from 'Components/Tooltip/Popover';
import { icons, kinds, tooltipPositions } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import styles from './QueueStatus.css';

function getDetailedPopoverBody(statusMessages) {
  return (
    <div>
      {
        statusMessages.map(({ title, messages }) => {
          return (
            <div
              key={title}
              className={messages.length ? undefined: styles.noMessages}
            >
              {title}
              <ul>
                {
                  messages.map((message) => {
                    return (
                      <li key={message}>
                        {message}
                      </li>
                    );
                  })
                }
              </ul>
            </div>
          );
        })
      }
    </div>
  );
}

function QueueStatus(props) {
  const {
    sourceTitle,
    status,
    trackedDownloadStatus,
    trackedDownloadState,
    statusMessages,
    errorMessage,
    position,
    canFlip
  } = props;

  const hasWarning = trackedDownloadStatus === 'warning';
  const hasError = trackedDownloadStatus === 'error';

  // status === 'downloading'
  let iconName = icons.DOWNLOADING;
  let iconKind = kinds.DEFAULT;
  let title = translate('Downloading');

  if (status === 'paused') {
    iconName = icons.PAUSED;
    title = translate('Paused');
  }

  if (status === 'queued') {
    iconName = icons.QUEUED;
    title = translate('Queued');
  }

  if (status === 'completed') {
    iconName = icons.DOWNLOADED;
    title = translate('Downloaded');

    if (trackedDownloadState === 'importPending') {
      title += ` - ${translate('WaitingToImport')}`;
      iconKind = kinds.PURPLE;
    }

    if (trackedDownloadState === 'importing') {
      title += ` - ${translate('Importing')}`;
      iconKind = kinds.PURPLE;
    }

    if (trackedDownloadState === 'failedPending') {
      title += ` - ${translate('WaitingToProcess')}`;
      iconKind = kinds.DANGER;
    }
  }

  if (hasWarning) {
    iconKind = kinds.WARNING;
  }

  if (status === 'delay') {
    iconName = icons.PENDING;
    title = translate('Pending');
  }

  if (status === 'downloadClientUnavailable') {
    iconName = icons.PENDING;
    iconKind = kinds.WARNING;
    title = translate('PendingDownloadClientUnavailable');
  }

  if (status === 'failed') {
    iconName = icons.DOWNLOADING;
    iconKind = kinds.DANGER;
    title = translate('DownloadFailed');
  }

  if (status === 'warning') {
    iconName = icons.DOWNLOADING;
    iconKind = kinds.WARNING;
    const warningMessage = errorMessage || translate('CheckDownloadClientForDetails');
    title = translate('DownloadWarning', { warningMessage });
  }

  if (hasError) {
    if (status === 'completed') {
      iconName = icons.DOWNLOAD;
      iconKind = kinds.DANGER;
      title = translate('ImportFailed', { sourceTitle });
    } else {
      iconName = icons.DOWNLOADING;
      iconKind = kinds.DANGER;
      title = translate('DownloadFailed');
    }
  }

  return (
    <Popover
      anchor={
        <Icon
          name={iconName}
          kind={iconKind}
        />
      }
      title={title}
      body={hasWarning || hasError ? getDetailedPopoverBody(statusMessages) : sourceTitle}
      position={position}
      canFlip={canFlip}
    />
  );
}

QueueStatus.propTypes = {
  sourceTitle: PropTypes.string.isRequired,
  status: PropTypes.string.isRequired,
  trackedDownloadStatus: PropTypes.string.isRequired,
  trackedDownloadState: PropTypes.string.isRequired,
  statusMessages: PropTypes.arrayOf(PropTypes.object),
  errorMessage: PropTypes.string,
  position: PropTypes.oneOf(tooltipPositions.all).isRequired,
  canFlip: PropTypes.bool.isRequired
};

QueueStatus.defaultProps = {
  trackedDownloadStatus: 'ok',
  trackedDownloadState: 'downloading',
  canFlip: false
};

export default QueueStatus;
