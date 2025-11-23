import { Filter } from 'Filters/Filter';

export default function getFilterValue<T>(
  filters: Filter[],
  filterKey: string | number,
  filterValueKey: string,
  defaultValue: T
) {
  const filter = filters.find((f) => f.key === filterKey);

  if (!filter) {
    return defaultValue;
  }

  const filterValue = filter.filters.find((f) => f.key === filterValueKey);

  return filterValue ? filterValue.value : defaultValue;
}
