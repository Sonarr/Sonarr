import moment from 'moment';

function isAfter(date, offsets = {}) {
  if (!date) {
    return false;
  }

  const offsetTime = moment();

  Object.keys(offsets).forEach((key) => {
    offsetTime.add(offsets[key], key);
  });

  return moment(date).isAfter(offsetTime);
}

export default isAfter;
