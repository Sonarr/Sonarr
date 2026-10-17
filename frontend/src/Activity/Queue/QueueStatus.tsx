import React from 'react';
import Icon from 'Components/Icon';
import Popover from 'Components/Tooltip/Popover';
import { TooltipPosition } from 'Helpers/Props/tooltipPositions';
import {
  QueueTrackedDownloadState,
  QueueTrackedDownloadStatus,
  StatusMessage,
} from 'typings/Queue';
import getQueueStatus from './getQueueStatus';
import QueueStatusMessages from './QueueStatusMessages';

interface QueueStatusProps {
  sourceTitle: string;
  status: string;
  trackedDownloadStatus?: QueueTrackedDownloadStatus;
  trackedDownloadState?: QueueTrackedDownloadState;
  statusMessages?: StatusMessage[];
  errorMessage?: string;
  position: TooltipPosition;
  canFlip?: boolean;
}

function QueueStatus(props: QueueStatusProps) {
  const {
    sourceTitle,
    status,
    trackedDownloadStatus,
    trackedDownloadState,
    statusMessages = [],
    errorMessage,
    position,
    canFlip = false,
  } = props;

  const { iconName, iconKind, title, hasWarning, hasError } = getQueueStatus({
    sourceTitle,
    status,
    trackedDownloadStatus,
    trackedDownloadState,
    errorMessage,
  });

  return (
    <Popover
      anchor={<Icon name={iconName} kind={iconKind} />}
      accessibleLabel={title}
      title={title}
      body={
        hasWarning || hasError ? (
          <QueueStatusMessages statusMessages={statusMessages} />
        ) : (
          sourceTitle
        )
      }
      position={position}
      canFlip={canFlip}
    />
  );
}

export default QueueStatus;
