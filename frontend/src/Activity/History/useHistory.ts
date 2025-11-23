import { keepPreviousData, useQueryClient } from '@tanstack/react-query';
import { useCallback, useMemo, useState } from 'react';
import { Filter, FilterBuilderProp } from 'Filters/Filter';
import { useCustomFiltersList } from 'Filters/useCustomFilters';
import useApiMutation from 'Helpers/Hooks/useApiMutation';
import usePage from 'Helpers/Hooks/usePage';
import usePagedApiQuery from 'Helpers/Hooks/usePagedApiQuery';
import { filterBuilderValueTypes } from 'Helpers/Props';
import History from 'typings/History';
import findSelectedFilters from 'Utilities/Filter/findSelectedFilters';
import translate from 'Utilities/String/translate';
import { useHistoryOptions } from './historyOptionsStore';

export const FILTERS: Filter[] = [
  {
    key: 'all',
    label: () => translate('All'),
    filters: [],
  },
  {
    key: 'grabbed',
    label: () => translate('Grabbed'),
    filters: [
      {
        key: 'eventType',
        value: '1',
        type: 'equal',
      },
    ],
  },
  {
    key: 'imported',
    label: () => translate('Imported'),
    filters: [
      {
        key: 'eventType',
        value: '3',
        type: 'equal',
      },
    ],
  },
  {
    key: 'failed',
    label: () => translate('Failed'),
    filters: [
      {
        key: 'eventType',
        value: '4',
        type: 'equal',
      },
    ],
  },
  {
    key: 'deleted',
    label: () => translate('Deleted'),
    filters: [
      {
        key: 'eventType',
        value: '5',
        type: 'equal',
      },
    ],
  },
  {
    key: 'renamed',
    label: () => translate('Renamed'),
    filters: [
      {
        key: 'eventType',
        value: '6',
        type: 'equal',
      },
    ],
  },
  {
    key: 'ignored',
    label: () => translate('Ignored'),
    filters: [
      {
        key: 'eventType',
        value: '7',
        type: 'equal',
      },
    ],
  },
];

export const FILTER_BUILDER: FilterBuilderProp<History>[] = [
  {
    name: 'eventType',
    label: () => translate('EventType'),
    type: 'equal',
    valueType: filterBuilderValueTypes.HISTORY_EVENT_TYPE,
  },
  {
    name: 'seriesIds',
    label: () => translate('Series'),
    type: 'equal',
    valueType: filterBuilderValueTypes.SERIES,
  },
  {
    name: 'quality',
    label: () => translate('Quality'),
    type: 'equal',
    valueType: filterBuilderValueTypes.QUALITY,
  },
  {
    name: 'languages',
    label: () => translate('Languages'),
    type: 'contains',
    valueType: filterBuilderValueTypes.LANGUAGE,
  },
];

const useHistory = () => {
  const { page, goToPage } = usePage('history');
  const { pageSize, selectedFilterKey, sortKey, sortDirection } =
    useHistoryOptions();
  const customFilters = useCustomFiltersList('history');

  const filters = useMemo(() => {
    return findSelectedFilters(selectedFilterKey, FILTERS, customFilters);
  }, [selectedFilterKey, customFilters]);

  const { refetch, ...query } = usePagedApiQuery<History>({
    path: '/history',
    page,
    pageSize,
    filters,
    sortKey,
    sortDirection,
    queryOptions: {
      placeholderData: keepPreviousData,
    },
  });

  const handleGoToPage = useCallback(
    (page: number) => {
      goToPage(page);
    },
    [goToPage]
  );

  return {
    ...query,
    goToPage: handleGoToPage,
    page,
    refetch,
  };
};

export default useHistory;

export const useFilters = () => {
  return FILTERS;
};

export const useMarkAsFailed = (id: number) => {
  const queryClient = useQueryClient();
  const [error, setError] = useState<string | null>(null);

  const { mutate, isPending } = useApiMutation<unknown, void>({
    path: `/history/failed/${id}`,
    method: 'POST',
    mutationOptions: {
      onMutate: () => {
        setError(null);
      },
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: ['/history'] });
      },
      onError: () => {
        setError('Error marking history item as failed');
      },
    },
  });

  return {
    markAsFailed: mutate,
    isMarkingAsFailed: isPending,
    markAsFailedError: error,
  };
};
