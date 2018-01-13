import moment from 'moment';
import formatTime from 'Utilities/Date/formatTime';
import isInNextWeek from 'Utilities/Date/isInNextWeek';
import isToday from 'Utilities/Date/isToday';
import isTomorrow from 'Utilities/Date/isTomorrow';
import isYesterday from 'Utilities/Date/isYesterday';

function getRelativeDate(date, shortDateFormat, showRelativeDates, { timeFormat, includeSeconds = false, timeForToday = false } = {}) {
  if (!date) {
    return null;
  }

  if (!showRelativeDates) {
    return moment(date).format(shortDateFormat);
  }

  if (isYesterday(date)) {
    return 'Yesterday';
  }

  if (isToday(date)) {
    if (timeForToday && timeFormat) {
      return formatTime(date, timeFormat, { includeMinuteZero: true, includeSeconds });
    }

    return 'Today';
  }

  if (isTomorrow(date)) {
    return 'Tomorrow';
  }

  if (isInNextWeek(date)) {
    return moment(date).format('dddd');
  }

  return moment(date).format(shortDateFormat);
}

export default getRelativeDate;
