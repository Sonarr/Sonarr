import moment from 'moment';

function formatTime(date, timeFormat, { includeMinuteZero = false, includeSeconds = false } = {}) {
  if (!date) {
    return '';
  }

  const time = moment(date);

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
