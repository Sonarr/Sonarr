import moment, { MomentInput } from 'moment';

function isInNextWeek(date: MomentInput) {
  if (!date) {
    return false;
  }
  const now = moment();
  return moment(date).isBetween(now, now.clone().add(6, 'days').endOf('day'));
}

export default isInNextWeek;
