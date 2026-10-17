import { IconKind, IconName } from 'Components/Icon';
import { icons, kinds } from 'Helpers/Props';
import {
  QueueTrackedDownloadState,
  QueueTrackedDownloadStatus,
} from 'typings/Queue';
import translate from 'Utilities/String/translate';

interface GetQueueStatusOptions {
  sourceTitle: string;
  status: string;
  trackedDownloadStatus?: QueueTrackedDownloadStatus;
  trackedDownloadState?: QueueTrackedDownloadState;
  errorMessage?: string;
}

export interface QueueStatusInfo {
  iconName: IconName;
  iconKind: IconKind;
  title: string;
  hasWarning: boolean;
  hasError: boolean;
}

function getQueueStatus({
  sourceTitle,
  status,
  trackedDownloadStatus = 'ok',
  trackedDownloadState = 'downloading',
  errorMessage,
}: GetQueueStatusOptions): QueueStatusInfo {
  const hasWarning = trackedDownloadStatus === 'warning';
  const hasError = trackedDownloadStatus === 'error';

  let iconName = icons.DOWNLOADING;
  let iconKind: IconKind = kinds.DEFAULT;
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
      iconKind = kinds.PRIMARY;
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

  return { iconName, iconKind, title, hasWarning, hasError };
}

export default getQueueStatus;
