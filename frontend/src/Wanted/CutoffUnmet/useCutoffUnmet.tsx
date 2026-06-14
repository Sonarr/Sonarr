import { keepPreviousData } from '@tanstack/react-query';
import { useEffect, useMemo } from 'react';
import Episode from 'Episode/Episode';
import { setEpisodeQueryKey } from 'Episode/useEpisode';
import { Filter, FilterBuilderProp } from 'Filters/Filter';
import { useCustomFiltersList } from 'Filters/useCustomFilters';
import usePage from 'Helpers/Hooks/usePage';
import usePagedApiQuery from 'Helpers/Hooks/usePagedApiQuery';
import { filterBuilderValueTypes } from 'Helpers/Props';
import findSelectedFilters from 'Utilities/Filter/findSelectedFilters';
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

export const FILTER_BUILDER: FilterBuilderProp<Episode>[] = [
  {
    name: 'monitored',
    label: () => translate('Monitored'),
    type: 'equal',
    valueType: filterBuilderValueTypes.BOOL,
  },
  {
    name: 'seriesIds',
    label: () => translate('Series'),
    type: 'equal',
    valueType: filterBuilderValueTypes.SERIES,
  },
  {
    name: 'qualityProfileIds',
    label: () => translate('QualityProfile'),
    type: 'equal',
    valueType: filterBuilderValueTypes.QUALITY_PROFILE,
  },
  {
    name: 'seriesType',
    label: () => translate('SeriesType'),
    type: 'equal',
    valueType: filterBuilderValueTypes.SERIES_TYPES,
  },
  {
    name: 'seriesTags',
    label: () => translate('Tags'),
    type: 'array',
    valueType: filterBuilderValueTypes.TAG,
  },
  {
    name: 'quality',
    label: () => translate('Quality'),
    type: 'equal',
    valueType: filterBuilderValueTypes.QUALITY,
  },
];

const useCutoffUnmet = () => {
  const { page, goToPage } = usePage('cutoffUnmet');
  const { pageSize, selectedFilterKey, sortKey, sortDirection } =
    useCutoffUnmetOptions();
  const customFilters = useCustomFiltersList('wanted.cutoffUnmet');

  const filters = useMemo(() => {
    return findSelectedFilters(selectedFilterKey, FILTERS, customFilters);
  }, [selectedFilterKey, customFilters]);

  const { isPlaceholderData, queryKey, ...query } = usePagedApiQuery<Episode>({
    path: '/wanted/cutoff',
    page,
    pageSize,
    filters,
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
    filters,
  };
};

export default useCutoffUnmet;

export const useFilters = () => {
  return FILTERS;
};
