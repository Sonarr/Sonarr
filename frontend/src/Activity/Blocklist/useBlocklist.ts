import { keepPreviousData, useQueryClient } from '@tanstack/react-query';
import { useMemo } from 'react';
import { useSelector } from 'react-redux';
import { CustomFilter, Filter, FilterBuilderProp } from 'App/State/AppState';
import useApiMutation from 'Helpers/Hooks/useApiMutation';
import usePage from 'Helpers/Hooks/usePage';
import usePagedApiQuery from 'Helpers/Hooks/usePagedApiQuery';
import { filterBuilderValueTypes } from 'Helpers/Props';
import { createCustomFiltersSelector } from 'Store/Selectors/createClientSideCollectionSelector';
import Blocklist from 'typings/Blocklist';
import findSelectedFilters from 'Utilities/Filter/findSelectedFilters';
import translate from 'Utilities/String/translate';
import { useBlocklistOptions } from './blocklistOptionsStore';

interface BulkBlocklistData {
  ids: number[];
}

export const FILTERS: Filter[] = [
  {
    key: 'all',
    label: () => translate('All'),
    filters: [],
  },
];

export const FILTER_BUILDER: FilterBuilderProp<Blocklist>[] = [
  {
    name: 'seriesIds',
    label: () => translate('Series'),
    type: 'equal',
    valueType: filterBuilderValueTypes.SERIES,
  },
  {
    name: 'protocols',
    label: () => translate('Protocol'),
    type: 'equal',
    valueType: filterBuilderValueTypes.PROTOCOL,
  },
];

const useBlocklist = () => {
  const { page, goToPage } = usePage('blocklist');
  const { pageSize, selectedFilterKey, sortKey, sortDirection } =
    useBlocklistOptions();
  const customFilters = useSelector(
    createCustomFiltersSelector('blocklist')
  ) as CustomFilter[];

  const filters = useMemo(() => {
    return findSelectedFilters(selectedFilterKey, FILTERS, customFilters);
  }, [selectedFilterKey, customFilters]);

  const { refetch, ...query } = usePagedApiQuery<Blocklist>({
    path: '/blocklist',
    page,
    pageSize,
    filters,
    sortKey,
    sortDirection,
    queryOptions: {
      placeholderData: keepPreviousData,
    },
  });

  return {
    ...query,
    goToPage,
    page,
    refetch,
  };
};

export default useBlocklist;

export const useFilters = () => {
  return FILTERS;
};

export const useRemoveBlocklistItem = (id: number) => {
  const queryClient = useQueryClient();

  const { mutate, isPending } = useApiMutation<unknown, void>({
    path: `/blocklist/${id}`,
    method: 'DELETE',
    mutationOptions: {
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: ['/blocklist'] });
      },
    },
  });

  return {
    removeBlocklistItem: mutate,
    isRemoving: isPending,
  };
};

export const useRemoveBlocklistItems = () => {
  const queryClient = useQueryClient();

  const { mutate, isPending } = useApiMutation<unknown, BulkBlocklistData>({
    path: `/blocklist/bulk`,
    method: 'DELETE',
    mutationOptions: {
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: ['/blocklist'] });
      },
    },
  });

  return {
    removeBlocklistItems: mutate,
    isRemoving: isPending,
  };
};
