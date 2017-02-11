import _ from 'lodash';
import { createSelector } from 'reselect';
import { filterTypes, sortDirections } from 'Helpers/Props';

const filterTypePredicates = {
  [filterTypes.CONTAINS]: function(value, filterValue) {
    return value.toLowerCase().indexOf(filterValue.toLowerCase()) > -1;
  },

  [filterTypes.EQUAL]: function(value, filterValue) {
    return value === filterValue;
  },

  [filterTypes.GREATER_THAN]: function(value, filterValue) {
    return value > filterValue;
  },

  [filterTypes.GREATER_THAN_OR_EQUAL]: function(value, filterValue) {
    return value >= filterValue;
  },

  [filterTypes.LESS_THAN]: function(value, filterValue) {
    return value < filterValue;
  },

  [filterTypes.LESS_THAN_OR_EQUAL]: function(value, filterValue) {
    return value <= filterValue;
  },

  [filterTypes.NOT_EQUAL]: function(value, filterValue) {
    return value !== filterValue;
  }
};

function getSortClause(sortKey, sortDirection, sortPredicates) {
  if (sortPredicates && sortPredicates.hasOwnProperty(sortKey)) {
    return function(item) {
      return sortPredicates[sortKey](item, sortDirection);
    };
  }

  return function(item) {
    return item[sortKey];
  };
}

function filter(items, state) {
  const {
    filterKey,
    filterValue,
    filterType,
    filterPredicates
  } = state;

  if (!filterKey || !filterValue) {
    return items;
  }

  return _.filter(items, (item) => {
    if (filterPredicates && filterPredicates.hasOwnProperty(filterKey)) {
      return filterPredicates[filterKey](item);
    }

    if (item.hasOwnProperty(filterKey)) {
      return filterTypePredicates[filterType](item[filterKey], filterValue);
    }

    return false;
  });
}

function sort(items, state) {
  const {
    sortKey,
    sortDirection,
    sortPredicates,
    secondarySortKey,
    secondarySortDirection
  } = state;

  const clauses = [];
  const orders = [];

  clauses.push(getSortClause(sortKey, sortDirection, sortPredicates));
  orders.push(sortDirection === sortDirections.ASCENDING ? 'asc' : 'desc');

  if (secondarySortKey &&
      secondarySortDirection &&
      (sortKey !== secondarySortKey ||
       sortDirection !== secondarySortDirection)) {
    clauses.push(getSortClause(secondarySortKey, secondarySortDirection, sortPredicates));
    orders.push(secondarySortDirection === sortDirections.ASCENDING ? 'asc' : 'desc');
  }

  return _.orderBy(items, clauses, orders);
}

function createClientSideCollectionSelector() {
  return createSelector(
    (state, { section }) => _.get(state, section),
    (state, { uiSection }) => _.get(state, uiSection),
    (sectionState, uiSectionState = {}) => {
      const state = Object.assign({}, sectionState, uiSectionState);

      const filtered = filter(state.items, state);
      const sorted = sort(filtered, state);

      return {
        ...sectionState,
        ...uiSectionState,
        items: sorted
      };
    }
  );
}

export default createClientSideCollectionSelector;
