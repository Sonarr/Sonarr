import formatTime from 'Utilities/Date/formatTime';
import isInNextWeek from 'Utilities/Date/isInNextWeek';
import isToday from 'Utilities/Date/isToday';
import isTomorrow from 'Utilities/Date/isTomorrow';
import isYesterday from 'Utilities/Date/isYesterday';
import translate from 'Utilities/String/translate';
import { convertToTimezone } from './convertToTimezone';
import formatDateTime from './formatDateTime';

interface GetRelativeDateOptions {
  date?: string;
  shortDateFormat: string;
  showRelativeDates: boolean;
  timeFormat?: string;
  timeZone?: string;
  includeSeconds?: boolean;
  timeForToday?: boolean;
  includeTime?: boolean;
}

function getRelativeDate({
  date,
  shortDateFormat,
  showRelativeDates,
  timeFormat,
  timeZone = '',
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
        timeZone,
      })
    : '';

  if (isTodayDate && timeForToday) {
    return time;
  }

  if (!showRelativeDates) {
    const dateTime = convertToTimezone(date, timeZone);
    return dateTime.format(shortDateFormat);
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
    const dateTime = convertToTimezone(date, timeZone);
    const day = dateTime.format('dddd');

    return includeTime ? translate('DayOfWeekAt', { day, time }) : day;
  }

  return includeTime && timeFormat
    ? formatDateTime(date, shortDateFormat, timeFormat, {
        includeSeconds,
        timeZone,
      })
    : convertToTimezone(date, timeZone).format(shortDateFormat);
}

export default getRelativeDate;
