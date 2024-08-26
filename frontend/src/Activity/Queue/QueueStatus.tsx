import React from 'react';
import Icon, { IconProps } from 'Components/Icon';
import Popover from 'Components/Tooltip/Popover';
import { icons, kinds } from 'Helpers/Props';
import TooltipPosition from 'Helpers/Props/TooltipPosition';
import {
  QueueTrackedDownloadState,
  QueueTrackedDownloadStatus,
  StatusMessage,
} from 'typings/Queue';
import translate from 'Utilities/String/translate';
import styles from './QueueStatus.css';

function getDetailedPopoverBody(statusMessages: StatusMessage[]) {
  return (
    <div>
      {statusMessages.map(({ title, messages }) => {
        return (
          <div
            key={title}
            className={messages.length ? undefined : styles.noMessages}
          >
            {title}
            <ul>
              {messages.map((message) => {
                return <li key={message}>{message}</li>;
              })}
            </ul>
          </div>
        );
      })}
    </div>
  );
}

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
    trackedDownloadStatus = 'ok',
    trackedDownloadState = 'downloading',
    statusMessages = [],
    errorMessage,
    position,
    canFlip = false,
  } = props;

  const hasWarning = trackedDownloadStatus === 'warning';
  const hasError = trackedDownloadStatus === 'error';

  // status === 'downloading'
  let iconName = icons.DOWNLOADING;
  let iconKind: IconProps['kind'] = kinds.DEFAULT;
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

    if (trackedDownloadState === 'importBlocked') {
      title += ` - ${translate('UnableToImportAutomatically')}`;
      iconKind = kinds.WARNING;
    }

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
    const warningMessage =
      errorMessage || translate('CheckDownloadClientForDetails');
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
      anchor={<Icon name={iconName} kind={iconKind} />}
      title={title}
      body={
        hasWarning || hasError
          ? getDetailedPopoverBody(statusMessages)
          : sourceTitle
      }
      position={position}
      canFlip={canFlip}
    />
  );
}

export default QueueStatus;
