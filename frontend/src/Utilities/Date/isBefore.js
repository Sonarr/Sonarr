import moment from 'moment';

function isBefore(date, offsets = {}) {
  if (!date) {
    return false;
  }

  const offsetTime = moment();

  Object.keys(offsets).forEach((key) => {
    offsetTime.add(offsets[key], key);
  });

  return moment(date).isBefore(offsetTime);
}

export default isBefore;
