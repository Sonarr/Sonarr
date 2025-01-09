import moment, { MomentInput } from 'moment';
import { FilterDateType } from 'Helpers/Props/filterTypes';

function isBefore(
  date: MomentInput,
  offsets: Partial<Record<FilterDateType, number>> = {}
) {
  if (!date) {
    return false;
  }

  const offsetTime = moment();

  Object.entries(offsets).forEach(([key, value]) => {
    offsetTime.add(value, key as FilterDateType);
  });

  return moment(date).isBefore(offsetTime);
}

export default isBefore;
