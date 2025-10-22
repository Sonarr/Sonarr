import moment from 'moment-timezone';
import { convertToTimezone } from './convertToTimezone';

function formatTime(
  date: moment.MomentInput,
  timeFormat: string,
  { includeMinuteZero = false, includeSeconds = false, timeZone = '' } = {}
) {
  if (!date) {
    return '';
  }

  const time = convertToTimezone(date, timeZone);

  if (includeSeconds) {
    timeFormat = timeFormat.replace(/\(?:mm\)?/, ':mm:ss');
  } else if (includeMinuteZero || time.minute() !== 0) {
    timeFormat = timeFormat.replace('(:mm)', ':mm');
  } else {
    timeFormat = timeFormat.replace('(:mm)', '');
  }

  return time.format(timeFormat);
}

export default formatTime;
