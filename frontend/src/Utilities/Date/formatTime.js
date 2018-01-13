import moment from 'moment';

function formatTime(date, timeFormat, { includeMinuteZero = false, includeSeconds = false } = {}) {
  if (!date) {
    return '';
  }

  if (includeSeconds) {
    timeFormat = timeFormat.replace(/\(?:mm\)?/, ':mm:ss');
  } else if (includeMinuteZero) {
    timeFormat = timeFormat.replace('(:mm)', ':mm');
  } else {
    timeFormat = timeFormat.replace('(:mm)', '');
  }

  return moment(date).format(timeFormat);
}

export default formatTime;
