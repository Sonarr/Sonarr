/* eslint max-params: 0 */
import moment from 'moment';

function getStatusStyle(hasFile, downloading, startTime, endTime, isMonitored) {
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
