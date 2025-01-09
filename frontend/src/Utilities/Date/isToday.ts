import moment, { MomentInput } from 'moment';

function isToday(date: MomentInput) {
  if (!date) {
    return false;
  }

  return moment(date).isSame(moment(), 'day');
}

export default isToday;
