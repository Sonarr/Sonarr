import PropTypes from 'prop-types';
import React from 'react';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import { tooltipPositions } from 'Helpers/Props';
import QueueStatus from './QueueStatus';
import styles from './QueueStatusCell.css';

function QueueStatusCell(props) {
  const {
    sourceTitle,
    status,
    trackedDownloadStatus,
    trackedDownloadState,
    statusMessages,
    errorMessage
  } = props;

  return (
    <TableRowCell className={styles.status}>
      <QueueStatus
        sourceTitle={sourceTitle}
        status={status}
        trackedDownloadStatus={trackedDownloadStatus}
        trackedDownloadState={trackedDownloadState}
        statusMessages={statusMessages}
        errorMessage={errorMessage}
        position={tooltipPositions.RIGHT}
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
  trackedDownloadStatus: 'ok',
  trackedDownloadState: 'downloading'
};

export default QueueStatusCell;
