import React from 'react';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import {
  QueueTrackedDownloadState,
  QueueTrackedDownloadStatus,
  StatusMessage,
} from 'typings/Queue';
import QueueStatus from './QueueStatus';
import styles from './QueueStatusCell.css';

interface QueueStatusCellProps {
  sourceTitle: string;
  status: string;
  trackedDownloadStatus?: QueueTrackedDownloadStatus;
  trackedDownloadState?: QueueTrackedDownloadState;
  statusMessages?: StatusMessage[];
  errorMessage?: string;
}

function QueueStatusCell(props: QueueStatusCellProps) {
  const {
    sourceTitle,
    status,
    trackedDownloadStatus = 'ok',
    trackedDownloadState = 'downloading',
    statusMessages,
    errorMessage,
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
        position="right"
      />
    </TableRowCell>
  );
}

export default QueueStatusCell;
