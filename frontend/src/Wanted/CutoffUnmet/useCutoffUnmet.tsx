import { keepPreviousData } from '@tanstack/react-query';
import { useEffect } from 'react';
import { Filter } from 'App/State/AppState';
import Episode from 'Episode/Episode';
import { setEpisodeQueryKey } from 'Episode/useEpisode';
import usePage from 'Helpers/Hooks/usePage';
import usePagedApiQuery from 'Helpers/Hooks/usePagedApiQuery';
import translate from 'Utilities/String/translate';
import { useCutoffUnmetOptions } from './cutoffUnmetOptionsStore';

export const FILTERS: Filter[] = [
  {
    key: 'monitored',
    label: () => translate('Monitored'),
    filters: [
      {
        key: 'monitored',
        value: [true],
        type: 'equal',
      },
    ],
  },
  {
    key: 'unmonitored',
    label: () => translate('Unmonitored'),
    filters: [
      {
        key: 'monitored',
        value: [false],
        type: 'equal',
      },
    ],
  },
];

const useCutoffUnmet = () => {
  const { page, goToPage } = usePage('cutoffUnmet');
  const { pageSize, selectedFilterKey, sortKey, sortDirection } =
    useCutoffUnmetOptions();

  const { isPlaceholderData, queryKey, ...query } = usePagedApiQuery<Episode>({
    path: '/wanted/cutoff',
    page,
    pageSize,
    queryParams: {
      monitored: selectedFilterKey === 'monitored',
    },
    sortKey,
    sortDirection,
    queryOptions: {
      placeholderData: keepPreviousData,
    },
  });

  useEffect(() => {
    if (!isPlaceholderData) {
      setEpisodeQueryKey('wanted.cutoffUnmet', queryKey);
    }
  }, [isPlaceholderData, queryKey]);

  return {
    ...query,
    goToPage,
    isPlaceholderData,
    page,
  };
};

export default useCutoffUnmet;
