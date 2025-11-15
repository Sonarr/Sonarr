import _ from 'lodash';
import ModelBase from 'App/ModelBase';
import { CustomFilter, Filter } from 'App/State/AppState';
import { filterTypes, sortDirections } from 'Helpers/Props';
import { FilterType } from 'Helpers/Props/filterTypes';
import getFilterTypePredicate from 'Helpers/Props/getFilterTypePredicate';
import findSelectedFilters from 'Utilities/Filter/findSelectedFilters';
import { SortDirection } from '../../Helpers/Props/sortDirections';

const getSortClause = <T, TSort = null>(
  sortKey: string,
  sortDirection: SortDirection,
  sortPredicates?: Record<
    keyof TSort,
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    (item: T, direction: SortDirection) => any
  >
) => {
  if (sortPredicates && sortPredicates.hasOwnProperty(sortKey)) {
    return function (item: T) {
      return sortPredicates[sortKey as keyof TSort](item, sortDirection);
    };
  }

  return function (item: T) {
    return item[sortKey as keyof T];
  };
};

const filter = <T extends ModelBase, TFilter = null, TSort = null>(
  data: T[],
  options: ClientSideFilterAndSortOptions<T, TFilter, TSort>
) => {
  const { selectedFilterKey, filters, customFilters, filterPredicates } =
    options;

  if (!selectedFilterKey) {
    return data;
  }

  const selectedFilters = findSelectedFilters(
    selectedFilterKey,
    filters,
    customFilters
  );

  return data.filter((item: T) => {
    let i = 0;
    let accepted = true;

    while (accepted && i < selectedFilters.length) {
      const { key, value, type = filterTypes.EQUAL } = selectedFilters[i];

      if (filterPredicates && filterPredicates.hasOwnProperty(key)) {
        const predicate = filterPredicates[key as keyof TFilter];

        if (Array.isArray(value)) {
          if (
            type === filterTypes.NOT_CONTAINS ||
            type === filterTypes.NOT_EQUAL
          ) {
            accepted = value.every((v) => predicate(item, v, type));
          } else {
            accepted = value.some((v) => predicate(item, v, type));
          }
        } else {
          accepted = predicate(item, value, type);
        }
      } else if (item.hasOwnProperty(key)) {
        const predicate = getFilterTypePredicate(type);

        if (Array.isArray(value)) {
          if (
            type === filterTypes.NOT_CONTAINS ||
            type === filterTypes.NOT_EQUAL
          ) {
            accepted = value.every((v) => predicate(item[key as keyof T], v));
          } else {
            accepted = value.some((v) => predicate(item[key as keyof T], v));
          }
        } else {
          accepted = predicate(item[key as keyof T], value);
        }
      } else {
        // Default to false if the filter can't be tested
        accepted = false;
      }

      i++;
    }

    return accepted;
  });
};

const sort = <T extends ModelBase, TFilter = null, TSort = null>(
  data: T[],
  options: ClientSideFilterAndSortOptions<T, TFilter, TSort>
) => {
  const {
    sortKey,
    sortDirection,
    sortPredicates,
    secondarySortKey,
    secondarySortDirection,
  } = options;

  const clauses = [];
  const orders: ('asc' | 'desc')[] = [];

  clauses.push(getSortClause(sortKey, sortDirection, sortPredicates));
  orders.push(sortDirection === sortDirections.ASCENDING ? 'asc' : 'desc');

  if (
    secondarySortKey &&
    secondarySortDirection &&
    (sortKey !== secondarySortKey || sortDirection !== secondarySortDirection)
  ) {
    clauses.push(
      getSortClause(secondarySortKey, secondarySortDirection, sortPredicates)
    );
    orders.push(
      secondarySortDirection === sortDirections.ASCENDING ? 'asc' : 'desc'
    );
  }

  return _.orderBy(data, clauses, orders);
};

interface ClientSideFilterAndSortOptions<T extends ModelBase, TFilter, TSort> {
  selectedFilterKey: string | number;
  filters: Filter[];
  filterPredicates?: Record<
    keyof TFilter,
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    (item: T, value: any, type: FilterType) => boolean
  >;
  customFilters: CustomFilter[];
  sortKey: string;
  sortDirection: SortDirection;
  secondarySortKey?: string;
  secondarySortDirection?: SortDirection;
  sortPredicates?: Record<
    keyof TSort,
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    (item: T, direction: SortDirection) => any
  >;
}

const clientSideFilterAndSort = <
  T extends ModelBase,
  TFilter = null,
  TSort = null
>(
  data: T[],
  options: ClientSideFilterAndSortOptions<T, TFilter, TSort>
) => {
  const filteredData = filter(data, options);
  const sortedData = sort(filteredData, options);

  return {
    data: sortedData,
    totalItems: data.length,
  };
};

export default clientSideFilterAndSort;
