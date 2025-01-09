import moment, { MomentInput } from 'moment';

function formatDate(date: MomentInput, dateFormat: string) {
  if (!date) {
    return '';
  }

  return moment(date).format(dateFormat);
}

export default formatDate;
