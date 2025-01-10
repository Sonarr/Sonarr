import { FilterType } from './filterTypes';

type FilterPredicate<T> = (itemValue: T, filterValue: T) => boolean;

function getFilterTypePredicate<T>(filterType: FilterType): FilterPredicate<T> {
  if (filterType === 'contains') {
    return function (itemValue, filterValue) {
      if (Array.isArray(itemValue)) {
        return itemValue.some((v) => v === filterValue);
      }

      if (typeof itemValue === 'string' && typeof filterValue === 'string') {
        return itemValue.toLowerCase().includes(filterValue.toLowerCase());
      }

      return false;
    };
  }

  if (filterType === 'equal') {
    return function (itemValue, filterValue) {
      return itemValue === filterValue;
    };
  }

  if (filterType === 'greaterThan') {
    return function (itemValue, filterValue) {
      return itemValue > filterValue;
    };
  }

  if (filterType === 'greaterThanOrEqual') {
    return function (itemValue, filterValue) {
      return itemValue >= filterValue;
    };
  }

  if (filterType === 'lessThan') {
    return function (itemValue, filterValue) {
      return itemValue < filterValue;
    };
  }

  if (filterType === 'lessThanOrEqual') {
    return function (itemValue, filterValue) {
      return itemValue <= filterValue;
    };
  }

  if (filterType === 'notContains') {
    return function (itemValue, filterValue) {
      if (Array.isArray(itemValue)) {
        return !itemValue.some((v) => v === filterValue);
      }

      if (typeof itemValue === 'string' && typeof filterValue === 'string') {
        return !itemValue.toLowerCase().includes(filterValue.toLowerCase());
      }

      return false;
    };
  }

  if (filterType === 'notEqual') {
    return function (itemValue, filterValue) {
      return itemValue !== filterValue;
    };
  }

  if (filterType === 'startsWith') {
    return function (itemValue, filterValue) {
      if (typeof itemValue === 'string' && typeof filterValue === 'string') {
        return itemValue.toLowerCase().startsWith(filterValue.toLowerCase());
      }

      return false;
    };
  }

  if (filterType === 'notStartsWith') {
    return function (itemValue, filterValue) {
      if (typeof itemValue === 'string' && typeof filterValue === 'string') {
        return !itemValue.toLowerCase().startsWith(filterValue.toLowerCase());
      }

      return false;
    };
  }

  if (filterType === 'endsWith') {
    return function (itemValue, filterValue) {
      if (typeof itemValue === 'string' && typeof filterValue === 'string') {
        return itemValue.toLowerCase().endsWith(filterValue.toLowerCase());
      }

      return false;
    };
  }

  if (filterType === 'notEndsWith') {
    return function (itemValue, filterValue) {
      if (typeof itemValue === 'string' && typeof filterValue === 'string') {
        return !itemValue.toLowerCase().endsWith(filterValue.toLowerCase());
      }

      return false;
    };
  }

  return () => {
    return false;
  };
}

export default getFilterTypePredicate;
