import moment from 'moment-timezone';

export const convertToTimezone = (date: moment.MomentInput, timeZone: string) => {
  if (!date) {
    return moment();
  }
  
  if (!timeZone) {
    return moment(date);
  }
  
  try {
    // Use moment-timezone to convert to the specified timezone
    return moment.tz(date, timeZone);
  } catch (error) {
    console.warn(`Error converting to timezone ${timeZone}. Using system timezone.`);
    return moment(date);
  }
};

export default convertToTimezone;
