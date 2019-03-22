export default function getFilterValue(filters, filterKey, filterValueKey, defaultValue) {
  const filter = filters.find((f) => f.key === filterKey);

  if (!filter) {
    return defaultValue;
  }

  const filterValue = filter.filters.find((f) => f.key === filterValueKey);

  return filterValue ? filterValue.value : defaultValue;
}
