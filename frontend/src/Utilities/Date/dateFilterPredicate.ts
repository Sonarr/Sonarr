import moment, { MomentInput } from 'moment';
import { FilterType } from 'Helpers/Props/filterTypes';
import isAfter from 'Utilities/Date/isAfter';
import isBefore from 'Utilities/Date/isBefore';

export default function (
  itemValue: MomentInput,
  filterValue: string | { time: string; value: number },
  type: FilterType
) {
  if (!itemValue) {
    return false;
  }

  if (typeof filterValue === 'string') {
    if (type === 'lessThan') {
      return moment(itemValue).isSame(filterValue);
    }

    if (type === 'greaterThan') {
      return moment(itemValue).isAfter(filterValue);
    }

    return false;
  }

  if (type === 'inLast') {
    return (
      isAfter(itemValue, { [filterValue.time]: filterValue.value * -1 }) &&
      isBefore(itemValue)
    );
  }

  if (type === 'notInLast') {
    return isBefore(itemValue, { [filterValue.time]: filterValue.value * -1 });
  }

  if (type === 'inNext') {
    return (
      isAfter(itemValue) &&
      isBefore(itemValue, { [filterValue.time]: filterValue.value })
    );
  }

  if (type === 'notInNext') {
    return isAfter(itemValue, { [filterValue.time]: filterValue.value });
  }

  return false;
}
