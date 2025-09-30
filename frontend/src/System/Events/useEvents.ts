import { keepPreviousData } from '@tanstack/react-query';
import { useMemo } from 'react';
import { Filter } from 'App/State/AppState';
import usePage from 'Helpers/Hooks/usePage';
import usePagedApiQuery from 'Helpers/Hooks/usePagedApiQuery';
import LogEvent from 'typings/LogEvent';
import translate from 'Utilities/String/translate';
import { useEventOptions } from './eventOptionsStore';

export const FILTERS: Filter[] = [
  {
    key: 'all',
    label: () => translate('All'),
    filters: [],
  },
  {
    key: 'info',
    label: () => translate('Info'),
    filters: [
      {
        key: 'level',
        value: 'info',
        type: 'equal',
      },
    ],
  },
  {
    key: 'warn',
    label: () => translate('Warn'),
    filters: [
      {
        key: 'level',
        value: 'warn',
        type: 'equal',
      },
    ],
  },
  {
    key: 'error',
    label: () => translate('Error'),
    filters: [
      {
        key: 'level',
        value: 'error',
        type: 'equal',
      },
    ],
  },
];

const useEvents = () => {
  const { page, goToPage } = usePage('events');
  const { pageSize, selectedFilterKey, sortKey, sortDirection } =
    useEventOptions();

  const filters = useMemo(() => {
    return FILTERS.find((f) => f.key === selectedFilterKey)?.filters;
  }, [selectedFilterKey]);

  const { refetch, ...query } = usePagedApiQuery<LogEvent>({
    path: '/log',
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

export default useEvents;

export const useFilters = () => {
  return FILTERS;
};
