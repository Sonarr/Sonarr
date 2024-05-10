import moment from 'moment';
import formatTime from 'Utilities/Date/formatTime';
import isInNextWeek from 'Utilities/Date/isInNextWeek';
import isToday from 'Utilities/Date/isToday';
import isTomorrow from 'Utilities/Date/isTomorrow';
import isYesterday from 'Utilities/Date/isYesterday';
import translate from 'Utilities/String/translate';
import formatDateTime from './formatDateTime';

function getRelativeDate(date, shortDateFormat, showRelativeDates, { timeFormat, includeSeconds = false, timeForToday = false, includeTime = false } = {}) {
  if (!date) {
    return null;
  }

  const isTodayDate = isToday(date);
  const time = formatTime(date, timeFormat, { includeMinuteZero: true, includeSeconds });

  if (isTodayDate && timeForToday && timeFormat) {
    return time;
  }

  if (!showRelativeDates) {
    return moment(date).format(shortDateFormat);
  }

  if (isYesterday(date)) {
    return includeTime ? translate('YesterdayAt', { time } ): translate('Yesterday');
  }

  if (isTodayDate) {
    return includeTime ? translate('TodayAt', { time } ): translate('Today');
  }

  if (isTomorrow(date)) {
    return includeTime ? translate('TomorrowAt', { time } ): translate('Tomorrow');
  }

  if (isInNextWeek(date)) {
    const day = moment(date).format('dddd');

    return includeTime ? translate('DayOfWeekAt', { day, time }) : day;
  }

  return includeTime ?
    formatDateTime(date, shortDateFormat, timeFormat, { includeSeconds }) :
    moment(date).format(shortDateFormat);
}

export default getRelativeDate;
