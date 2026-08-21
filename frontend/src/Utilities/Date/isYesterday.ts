import moment, { MomentInput } from 'moment-timezone';

function isYesterday(date: MomentInput) {
  if (!date) {
    return false;
  }

  return moment(date).isSame(moment().subtract(1, 'day'), 'day');
}

export default isYesterday;
