import moment, { MomentInput } from 'moment';

function isSameWeek(date: MomentInput) {
  if (!date) {
    return false;
  }

  return moment(date).isSame(moment(), 'week');
}

export default isSameWeek;
