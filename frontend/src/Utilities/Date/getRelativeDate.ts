import moment from 'moment-timezone';
import formatDateTime from 'Utilities/Date/formatDateTime';
import formatTime from 'Utilities/Date/formatTime';
import isInNextWeek from 'Utilities/Date/isInNextWeek';
import isToday from 'Utilities/Date/isToday';
import isTomorrow from 'Utilities/Date/isTomorrow';
import isYesterday from 'Utilities/Date/isYesterday';
import translate from 'Utilities/String/translate';

interface GetRelativeDateOptions {
  date?: string;
  shortDateFormat: string;
  showRelativeDates: boolean;
  timeFormat?: string;
  includeSeconds?: boolean;
  timeForToday?: boolean;
  includeTime?: boolean;
}

function getRelativeDate({
  date,
  shortDateFormat,
  showRelativeDates,
  timeFormat,
  includeSeconds = false,
  timeForToday = false,
  includeTime = false,
}: GetRelativeDateOptions) {
  if (!date) {
    return '';
  }

  if ((includeTime || timeForToday) && !timeFormat) {
    throw new Error(
      "getRelativeDate: 'timeFormat' is required when 'includeTime' or 'timeForToday' is true"
    );
  }

  const isTodayDate = isToday(date);
  const time = timeFormat
    ? formatTime(date, timeFormat, {
        includeMinuteZero: true,
        includeSeconds,
      })
    : '';

  if (isTodayDate && timeForToday) {
    return time;
  }

  if (!showRelativeDates) {
    return moment(date).format(shortDateFormat);
  }

  if (isYesterday(date)) {
    return includeTime
      ? translate('YesterdayAt', { time })
      : translate('Yesterday');
  }

  if (isTodayDate) {
    return includeTime ? translate('TodayAt', { time }) : translate('Today');
  }

  if (isTomorrow(date)) {
    return includeTime
      ? translate('TomorrowAt', { time })
      : translate('Tomorrow');
  }

  if (isInNextWeek(date)) {
    const day = getDayOfWeek(moment(date).day());

    return includeTime ? translate('DayOfWeekAt', { day, time }) : day;
  }

  return includeTime && timeFormat
    ? formatDateTime(date, shortDateFormat, timeFormat, {
        includeSeconds,
      })
    : moment(date).format(shortDateFormat);
}

export default getRelativeDate;

function getDayOfWeek(dayNumber: number) {
  switch (dayNumber) {
    case 0:
      return translate('Sunday');
    case 1:
      return translate('Monday');
    case 2:
      return translate('Tuesday');
    case 3:
      return translate('Wednesday');
    case 4:
      return translate('Thursday');
    case 5:
      return translate('Friday');
    case 6:
      return translate('Saturday');
    default:
      return '';
  }
}
