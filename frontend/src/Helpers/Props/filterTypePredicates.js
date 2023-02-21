import * as filterTypes from './filterTypes';

const filterTypePredicates = {
  [filterTypes.CONTAINS]: function(itemValue, filterValue) {
    if (Array.isArray(itemValue)) {
      return itemValue.some((v) => v === filterValue);
    }

    return itemValue.toLowerCase().contains(filterValue.toLowerCase());
  },

  [filterTypes.EQUAL]: function(itemValue, filterValue) {
    return itemValue === filterValue;
  },

  [filterTypes.GREATER_THAN]: function(itemValue, filterValue) {
    return itemValue > filterValue;
  },

  [filterTypes.GREATER_THAN_OR_EQUAL]: function(itemValue, filterValue) {
    return itemValue >= filterValue;
  },

  [filterTypes.LESS_THAN]: function(itemValue, filterValue) {
    return itemValue < filterValue;
  },

  [filterTypes.LESS_THAN_OR_EQUAL]: function(itemValue, filterValue) {
    return itemValue <= filterValue;
  },

  [filterTypes.NOT_CONTAINS]: function(itemValue, filterValue) {
    if (Array.isArray(itemValue)) {
      return !itemValue.some((v) => v === filterValue);
    }

    return !itemValue.toLowerCase().contains(filterValue.toLowerCase());
  },

  [filterTypes.NOT_EQUAL]: function(itemValue, filterValue) {
    return itemValue !== filterValue;
  },

  [filterTypes.STARTS_WITH]: function(itemValue, filterValue) {
    return itemValue.toLowerCase().startsWith(filterValue.toLowerCase());
  },

  [filterTypes.NOT_STARTS_WITH]: function(itemValue, filterValue) {
    return !itemValue.toLowerCase().startsWith(filterValue.toLowerCase());
  },

  [filterTypes.ENDS_WITH]: function(itemValue, filterValue) {
    return itemValue.toLowerCase().endsWith(filterValue.toLowerCase());
  },

  [filterTypes.NOT_ENDS_WITH]: function(itemValue, filterValue) {
    return !itemValue.toLowerCase().endsWith(filterValue.toLowerCase());
  }
};

export default filterTypePredicates;
