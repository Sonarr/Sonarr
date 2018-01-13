import moment from 'moment';
import PropTypes from 'prop-types';
import React from 'react';
import { icons, kinds } from 'Helpers/Props';
import Icon from 'Components/Icon';

function QueueDetails(props) {
  const {
    title,
    size,
    sizeleft,
    estimatedCompletionTime,
    status: queueStatus,
    errorMessage,
    progressBar
  } = props;

  const status = queueStatus.toLowerCase();

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

    // TODO: show an icon when download is complete, but not imported yet?
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
  errorMessage: PropTypes.string,
  progressBar: PropTypes.node.isRequired
};

export default QueueDetails;
