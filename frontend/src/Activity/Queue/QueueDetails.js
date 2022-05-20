import moment from 'moment';
import PropTypes from 'prop-types';
import React from 'react';
import Icon from 'Components/Icon';
import { icons, kinds } from 'Helpers/Props';

function QueueDetails(props) {
  const {
    title,
    size,
    sizeleft,
    estimatedCompletionTime,
    status,
    trackedDownloadState,
    trackedDownloadStatus,
    errorMessage,
    progressBar
  } = props;

  const progress = (100 - sizeleft / size * 100);

  if (status === 'pending') {
    return (
      <Icon
        name={icons.PENDING}
        title={`Release will be processed ${moment(estimatedCompletionTime).fromNow()}`}
      />
    );
  }

  if (status === 'completed') {
    if (errorMessage) {
      return (
        <Icon
          name={icons.DOWNLOAD}
          kind={kinds.DANGER}
          title={`Import failed: ${errorMessage}`}
        />
      );
    }

    if (trackedDownloadStatus === 'warning') {
      return (
        <Icon
          name={icons.DOWNLOAD}
          kind={kinds.WARNING}
          title={'Downloaded - Unable to Import: check logs for details'}
        />
      );
    }

    if (trackedDownloadState === 'importPending') {
      return (
        <Icon
          name={icons.DOWNLOAD}
          kind={kinds.PURPLE}
          title={'Downloaded - Waiting to Import'}
        />
      );
    }

    if (trackedDownloadState === 'importing') {
      return (
        <Icon
          name={icons.DOWNLOAD}
          kind={kinds.PURPLE}
          title={'Downloaded - Importing'}
        />
      );
    }
  }

  if (errorMessage) {
    return (
      <Icon
        name={icons.DOWNLOADING}
        kind={kinds.DANGER}
        title={`Download failed: ${errorMessage}`}
      />
    );
  }

  if (status === 'failed') {
    return (
      <Icon
        name={icons.DOWNLOADING}
        kind={kinds.DANGER}
        title="Download failed: check download client for more details"
      />
    );
  }

  if (status === 'warning') {
    return (
      <Icon
        name={icons.DOWNLOADING}
        kind={kinds.WARNING}
        title="Download warning: check download client for more details"
      />
    );
  }

  if (progress < 5) {
    return (
      <Icon
        name={icons.DOWNLOADING}
        title={`Episode is downloading - ${progress.toFixed(1)}% ${title}`}
      />
    );
  }

  return progressBar;
}

QueueDetails.propTypes = {
  title: PropTypes.string.isRequired,
  size: PropTypes.number.isRequired,
  sizeleft: PropTypes.number.isRequired,
  estimatedCompletionTime: PropTypes.string,
  status: PropTypes.string.isRequired,
  trackedDownloadState: PropTypes.string.isRequired,
  trackedDownloadStatus: PropTypes.string.isRequired,
  errorMessage: PropTypes.string,
  progressBar: PropTypes.node.isRequired
};

export default QueueDetails;
