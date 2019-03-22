import moment from 'moment';
import isAfter from 'Utilities/Date/isAfter';
import isBefore from 'Utilities/Date/isBefore';
import * as filterTypes from 'Helpers/Props/filterTypes';

export default function(itemValue, filterValue, type) {
  if (!itemValue) {
    return false;
  }

  switch (type) {
    case filterTypes.LESS_THAN:
      return moment(itemValue).isBefore(filterValue);

    case filterTypes.GREATER_THAN:
      return moment(itemValue).isAfter(filterValue);

    case filterTypes.IN_LAST:
      return (
        isAfter(itemValue, { [filterValue.time]: filterValue.value * -1 }) &&
        isBefore(itemValue)
      );

    case filterTypes.IN_NEXT:
      return (
        isAfter(itemValue) &&
        isBefore(itemValue, { [filterValue.time]: filterValue.value })
      );

    default:
      return false;
  }
}
