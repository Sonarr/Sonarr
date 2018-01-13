import moment from 'moment';
import PropTypes from 'prop-types';
import React from 'react';
import formatTime from 'Utilities/Date/formatTime';
import isInNextWeek from 'Utilities/Date/isInNextWeek';
import isToday from 'Utilities/Date/isToday';
import isTomorrow from 'Utilities/Date/isTomorrow';
import { kinds, sizes } from 'Helpers/Props';
import Label from 'Components/Label';

function EpisodeAiring(props) {
  const {
    airDateUtc,
    network,
    shortDateFormat,
    showRelativeDates,
    timeFormat
  } = props;

  const networkLabel = (
    <Label
      kind={kinds.INFO}
      size={sizes.MEDIUM}
    >
      {network}
    </Label>
  );

  if (!airDateUtc) {
    return (
      <span>
        TBA on {networkLabel}
      </span>
    );
  }

  const time = formatTime(airDateUtc, timeFormat);

  if (!showRelativeDates) {
    return (
      <span>
        {moment(airDateUtc).format(shortDateFormat)} at {time} on {networkLabel}
      </span>
    );
  }

  if (isToday(airDateUtc)) {
    return (
      <span>
        {time} on {networkLabel}
      </span>
    );
  }

  if (isTomorrow(airDateUtc)) {
    return (
      <span>
        Tomorrow at {time} on {networkLabel}
      </span>
    );
  }

  if (isInNextWeek(airDateUtc)) {
    return (
      <span>
        {moment(airDateUtc).format('dddd')} at {time} on {networkLabel}
      </span>
    );
  }

  return (
    <span>
      {moment(airDateUtc).format(shortDateFormat)} at {time} on {networkLabel}
    </span>
  );
}

EpisodeAiring.propTypes = {
  airDateUtc: PropTypes.string.isRequired,
  network: PropTypes.string.isRequired,
  shortDateFormat: PropTypes.string.isRequired,
  showRelativeDates: PropTypes.bool.isRequired,
  timeFormat: PropTypes.string.isRequired
};

export default EpisodeAiring;
