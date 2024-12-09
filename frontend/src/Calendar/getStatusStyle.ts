import moment from 'moment';
import { CalendarStatus } from 'typings/Calendar';

function getStatusStyle(
  hasFile: boolean,
  downloading: boolean,
  startTime: moment.Moment,
  endTime: moment.Moment,
  isMonitored: boolean
): CalendarStatus {
  const currentTime = moment();

  if (hasFile) {
    return 'downloaded';
  }

  if (downloading) {
    return 'downloading';
  }

  if (!isMonitored) {
    return 'unmonitored';
  }

  if (currentTime.isAfter(startTime) && currentTime.isBefore(endTime)) {
    return 'onAir';
  }

  if (endTime.isBefore(currentTime) && !hasFile) {
    return 'missing';
  }

  return 'unaired';
}

export default getStatusStyle;
