import moment from 'moment';
import padNumber from 'Utilities/Number/padNumber';

function formatTimeSpan(timeSpan) {
  if (!timeSpan) {
    return '';
  }

  const duration = moment.duration(timeSpan);

  const days = Math.floor(duration.asDays());
  const hours = padNumber(duration.get('hours'), 2);
  const minutes = padNumber(duration.get('minutes'), 2);
  const seconds = padNumber(duration.get('seconds'), 2);

  const time = `${hours}:${minutes}:${seconds}`;

  if (days > 0) {
    return `${days}d ${time}`;
  }

  return time;
}

export default formatTimeSpan;
