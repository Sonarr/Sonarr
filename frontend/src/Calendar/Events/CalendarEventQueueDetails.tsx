import React from 'react';
import QueueDetails from 'Activity/Queue/QueueDetails';
import CircularProgressBar from 'Components/CircularProgressBar';
import {
  QueueTrackedDownloadState,
  QueueTrackedDownloadStatus,
  StatusMessage,
} from 'typings/Queue';

interface CalendarEventQueueDetailsProps {
  title: string;
  size: number;
  sizeleft: number;
  estimatedCompletionTime?: string;
  status: string;
  trackedDownloadState: QueueTrackedDownloadState;
  trackedDownloadStatus: QueueTrackedDownloadStatus;
  statusMessages?: StatusMessage[];
  errorMessage?: string;
}

function CalendarEventQueueDetails({
  title,
  size,
  sizeleft,
  estimatedCompletionTime,
  status,
  trackedDownloadState,
  trackedDownloadStatus,
  statusMessages,
  errorMessage,
}: CalendarEventQueueDetailsProps) {
  const progress = size ? 100 - (sizeleft / size) * 100 : 0;

  return (
    <QueueDetails
      title={title}
      size={size}
      sizeleft={sizeleft}
      estimatedCompletionTime={estimatedCompletionTime}
      status={status}
      trackedDownloadState={trackedDownloadState}
      trackedDownloadStatus={trackedDownloadStatus}
      statusMessages={statusMessages}
      errorMessage={errorMessage}
      progressBar={
        <CircularProgressBar
          progress={progress}
          size={20}
          strokeWidth={2}
          strokeColor="#7a43b6"
        />
      }
    />
  );
}

export default CalendarEventQueueDetails;
