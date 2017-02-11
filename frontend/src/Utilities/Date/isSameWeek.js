import moment from 'moment';

function isSameWeek(date) {
  if (!date) {
    return false;
  }

  return moment(date).isSame(moment(), 'week');
}

export default isSameWeek;
