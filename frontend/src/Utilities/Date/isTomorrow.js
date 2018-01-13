import moment from 'moment';

function isTomorrow(date) {
  if (!date) {
    return false;
  }

  return moment(date).isSame(moment().add(1, 'day'), 'day');
}

export default isTomorrow;
