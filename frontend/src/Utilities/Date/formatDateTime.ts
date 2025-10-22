import moment from 'moment-timezone';
import translate from 'Utilities/String/translate';
import { convertToTimezone } from './convertToTimezone';
import formatTime from './formatTime';
import isToday from './isToday';
import isTomorrow from './isTomorrow';
import isYesterday from './isYesterday';

function getRelativeDay(
  date: moment.MomentInput,
  includeRelativeDate: boolean
) {
  if (!includeRelativeDate) {
    return '';
  }

  if (isYesterday(date)) {
    return translate('Yesterday');
  }

  if (isToday(date)) {
    return translate('Today');
  }

  if (isTomorrow(date)) {
    return translate('Tomorrow');
  }

  return '';
}

function formatDateTime(
  date: moment.MomentInput,
  dateFormat: string,
  timeFormat: string,
  { includeSeconds = false, includeRelativeDay = false, timeZone = '' } = {}
) {
  if (!date) {
    return '';
  }

  const dateTime = convertToTimezone(date, timeZone);

  const relativeDay = getRelativeDay(dateTime, includeRelativeDay);
  const formattedDate = dateTime.format(dateFormat);
  const formattedTime = formatTime(dateTime, timeFormat, {
    includeMinuteZero: true,
    includeSeconds,
    timeZone,
  });

  if (relativeDay) {
    return translate('FormatDateTimeRelative', {
      relativeDay,
      formattedDate,
      formattedTime,
    });
  }
  return translate('FormatDateTime', { formattedDate, formattedTime });
}

export default formatDateTime;
