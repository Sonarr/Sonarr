import moment from 'moment';

function isToday(date) {
  if (!date) {
    return false;
  }

  return moment(date).isSame(moment(), 'day');
}

export default isToday;
