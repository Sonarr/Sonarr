import { CustomFilter, Filter } from 'App/State/AppState';

export default function findSelectedFilters(
  selectedFilterKey: string | number,
  filters: Filter[] = [],
  customFilters: CustomFilter[] = []
) {
  if (!selectedFilterKey) {
    return [];
  }

  let selectedFilter: Filter | CustomFilter | undefined = filters.find(
    (f) => f.key === selectedFilterKey
  );

  if (!selectedFilter) {
    selectedFilter = customFilters.find((f) => f.id === selectedFilterKey);
  }

  if (!selectedFilter) {
    // TODO: throw in dev
    console.error('Matching filter not found');
    return [];
  }

  return selectedFilter.filters;
}
