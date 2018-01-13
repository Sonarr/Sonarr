export default function findSelectedFilters(selectedFilterKey, filters = [], customFilters = []) {
  if (!selectedFilterKey) {
    return [];
  }

  const selectedFilter = [...filters, ...customFilters].find((f) => f.key === selectedFilterKey);

  if (!selectedFilter) {
    // TODO: throw in dev
    console.error('Matching filter not found');
    return [];
  }

  return selectedFilter.filters;
}
