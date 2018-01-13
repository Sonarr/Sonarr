export default function getFilterValue(filters, filterKey) {
  const filter = filters.find((f) => f.key === filterKey);

  return filter && filter.value;
}
