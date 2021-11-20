import PropTypes from 'prop-types';
import React from 'react';
import QueueDetails from 'Activity/Queue/QueueDetails';
import CircularProgressBar from 'Components/CircularProgressBar';

function CalendarEventQueueDetails(props) {
  const {
    title,
    size,
    sizeleft,
    estimatedCompletionTime,
    status,
    trackedDownloadState,
    trackedDownloadStatus,
    errorMessage
  } = props;

  const progress = size ? (100 - sizeleft / size * 100) : 0;

  return (
    <QueueDetails
      title={title}
      size={size}
      sizeleft={sizeleft}
      estimatedCompletionTime={estimatedCompletionTime}
      status={status}
      trackedDownloadState={trackedDownloadState}
      trackedDownloadStatus={trackedDownloadStatus}
      errorMessage={errorMessage}
      progressBar={
        <div title={`Episode is downloading - ${progress.toFixed(1)}% ${title}`}>
          <CircularProgressBar
            progress={progress}
            size={20}
            strokeWidth={2}
            strokeColor={'#7a43b6'}
          />
        </div>
      }
    />
  );
}

CalendarEventQueueDetails.propTypes = {
  title: PropTypes.string.isRequired,
  size: PropTypes.number.isRequired,
  sizeleft: PropTypes.number.isRequired,
  estimatedCompletionTime: PropTypes.string,
  status: PropTypes.string.isRequired,
  trackedDownloadState: PropTypes.string.isRequired,
  trackedDownloadStatus: PropTypes.string.isRequired,
  errorMessage: PropTypes.string
};

export default CalendarEventQueueDetails;
