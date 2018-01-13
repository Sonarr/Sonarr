import moment from 'moment';
import formatTime from './formatTime';
import isToday from './isToday';
import isTomorrow from './isTomorrow';
import isYesterday from './isYesterday';

function getRelativeDay(date, includeRelativeDate) {
  if (!includeRelativeDate) {
    return '';
  }

  if (isYesterday(date)) {
    return 'Yesterday, ';
  }

  if (isToday(date)) {
    return 'Today, ';
  }

  if (isTomorrow(date)) {
    return 'Tomorrow, ';
  }

  return '';
}

function formatDateTime(date, dateFormat, timeFormat, { includeSeconds = false, includeRelativeDay = false } = {}) {
  if (!date) {
    return '';
  }

  const relativeDay = getRelativeDay(date, includeRelativeDay);
  const formattedDate = moment(date).format(dateFormat);
  const formattedTime = formatTime(date, timeFormat, { includeMinuteZero: true, includeSeconds });

  return `${relativeDay}${formattedDate} ${formattedTime}`;
}

export default formatDateTime;
