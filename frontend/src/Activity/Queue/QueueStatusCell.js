import PropTypes from 'prop-types';
import React from 'react';
import { icons, kinds, tooltipPositions } from 'Helpers/Props';
import Icon from 'Components/Icon';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import Popover from 'Components/Tooltip/Popover';
import styles from './QueueStatusCell.css';

function getDetailedPopoverBody(statusMessages) {
  return (
    <div>
      {
        statusMessages.map(({ title, messages }) => {
          return (
            <div key={title}>
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

function QueueStatusCell(props) {
  const {
    sourceTitle,
    status,
    trackedDownloadStatus,
    trackedDownloadState,
    statusMessages,
    errorMessage
  } = props;

  const hasWarning = trackedDownloadStatus === 'warning';
  const hasError = trackedDownloadStatus === 'error';

  // status === 'downloading'
  let iconName = icons.DOWNLOADING;
  let iconKind = kinds.DEFAULT;
  let title = 'Downloading';

  if (hasWarning) {
    iconKind = kinds.WARNING;
  }

  if (status === 'paused') {
    iconName = icons.PAUSED;
    title = 'Paused';
  }

  if (status === 'queued') {
    iconName = icons.QUEUED;
    title = 'Queued';
  }

  if (status === 'completed') {
    iconName = icons.DOWNLOADED;
    title = 'Downloaded';

    if (trackedDownloadState === 'importPending') {
      title += ' - Waiting to Import';
    }

    if (trackedDownloadState === 'importing') {
      title += ' - Importing';
    }

    if (trackedDownloadState === 'failedPending') {
      title += ' - Waiting to Process';
    }
  }

  if (status === 'delay') {
    iconName = icons.PENDING;
    title = 'Pending';
  }

  if (status === 'downloadClientUnavailable') {
    iconName = icons.PENDING;
    iconKind = kinds.WARNING;
    title = 'Pending - Download client is unavailable';
  }

  if (status === 'failed') {
    iconName = icons.DOWNLOADING;
    iconKind = kinds.DANGER;
    title = 'Download failed';
  }

  if (status === 'warning') {
    iconName = icons.DOWNLOADING;
    iconKind = kinds.WARNING;
    title = `Download warning: ${errorMessage || 'check download client for more details'}`;
  }

  if (hasError) {
    if (status === 'completed') {
      iconName = icons.DOWNLOAD;
      iconKind = kinds.DANGER;
      title = `Import failed: ${sourceTitle}`;
    } else {
      iconName = icons.DOWNLOADING;
      iconKind = kinds.DANGER;
      title = 'Download failed';
    }
  }

  return (
    <TableRowCell className={styles.status}>
      <Popover
        anchor={
          <Icon
            name={iconName}
            kind={iconKind}
          />
        }
        title={title}
        body={hasWarning || hasError ? getDetailedPopoverBody(statusMessages) : sourceTitle}
        position={tooltipPositions.RIGHT}
        canFlip={false}
      />
    </TableRowCell>
  );
}

QueueStatusCell.propTypes = {
  sourceTitle: PropTypes.string.isRequired,
  status: PropTypes.string.isRequired,
  trackedDownloadStatus: PropTypes.string.isRequired,
  trackedDownloadState: PropTypes.string.isRequired,
  statusMessages: PropTypes.arrayOf(PropTypes.object),
  errorMessage: PropTypes.string
};

QueueStatusCell.defaultProps = {
  trackedDownloadStatus: 'Ok',
  trackedDownloadState: 'Downloading'
};

export default QueueStatusCell;
