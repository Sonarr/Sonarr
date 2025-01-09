import moment, { MomentInput } from 'moment';
import translate from 'Utilities/String/translate';
import formatTime from './formatTime';
import isToday from './isToday';
import isTomorrow from './isTomorrow';
import isYesterday from './isYesterday';

function getRelativeDay(date: MomentInput, includeRelativeDate: boolean) {
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
  date: MomentInput,
  dateFormat: string,
  timeFormat: string,
  { includeSeconds = false, includeRelativeDay = false } = {}
) {
  if (!date) {
    return '';
  }

  const relativeDay = getRelativeDay(date, includeRelativeDay);
  const formattedDate = moment(date).format(dateFormat);
  const formattedTime = formatTime(date, timeFormat, {
    includeMinuteZero: true,
    includeSeconds,
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
