import moment, { DurationInputArg1 } from 'moment-timezone';
import formatUnit from 'Utilities/Number/formatUnit';

function formatShortTimeSpan(timeSpan: DurationInputArg1) {
  if (!timeSpan) {
    return '';
  }

  const duration = moment.duration(timeSpan);

  const hours = Math.floor(duration.asHours());
  const minutes = Math.floor(duration.asMinutes());
  const seconds = Math.floor(duration.asSeconds());

  if (hours > 0) {
    return formatUnit(hours, 'hour');
  }

  if (minutes > 0) {
    return formatUnit(minutes, 'minute');
  }

  return formatUnit(seconds, 'second');
}

export default formatShortTimeSpan;
