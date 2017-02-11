import moment from 'moment';

function formatDate(date, dateFormat) {
  if (!date) {
    return '';
  }

  return moment(date).format(dateFormat);
}

export default formatDate;
