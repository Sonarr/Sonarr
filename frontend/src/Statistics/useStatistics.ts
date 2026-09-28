import { keepPreviousData } from '@tanstack/react-query';
import { useMemo } from 'react';
import { Filter, FilterBuilderProp, PropertyFilter } from 'Filters/Filter';
import { useCustomFiltersList } from 'Filters/useCustomFilters';
import useApiQuery from 'Helpers/Hooks/useApiQuery';
import { filterBuilderValueTypes } from 'Helpers/Props';
import * as filterTypes from 'Helpers/Props/filterTypes';
import Quality from 'Quality/Quality';
import { QueryParams } from 'Utilities/Fetch/getQueryString';
import findSelectedFilters from 'Utilities/Filter/findSelectedFilters';
import translate from 'Utilities/String/translate';
import { useStatisticsOption } from './statisticsOptionsStore';

export interface QualityProfileStatistics {
  qualityProfileId: number;
  name: string;
  seriesCount: number;
  episodeFileCount: number;
  sizeOnDisk: number;
}

export interface QualityStatistics {
  quality: Quality;
  episodeFileCount: number;
  sizeOnDisk: number;
}

export interface TagStatistics {
  tagId: number;
  label: string;
  seriesCount: number;
  episodeFileCount: number;
  sizeOnDisk: number;
}

export interface Statistics {
  seriesCount: number;
  monitoredSeriesCount: number;
  completedSeriesCount: number;
  continuingSeriesCount: number;
  endedSeriesCount: number;
  upcomingSeriesCount: number;
  deletedSeriesCount: number;
  standardSeriesCount: number;
  dailySeriesCount: number;
  animeSeriesCount: number;
  seasonCount: number;
  completedSeasonCount: number;
  totalEpisodeCount: number;
  monitoredEpisodeCount: number;
  downloadedEpisodeCount: number;
  missingEpisodeCount: number;
  unairedEpisodeCount: number;
  episodeFileCount: number;
  sizeOnDisk: number;
  qualityProfileStatistics: QualityProfileStatistics[];
  qualityStatistics: QualityStatistics[];
  tagStatistics: TagStatistics[];
}

export const FILTERS: Filter[] = [
  {
    key: 'all',
    label: () => translate('All'),
    filters: [],
  },
];

export const FILTER_BUILDER: FilterBuilderProp<Statistics>[] = [
  {
    name: 'rootFolderPath',
    label: () => translate('RootFolderPath'),
    type: 'exact',
  },
  {
    name: 'monitored',
    label: () => translate('Monitored'),
    type: 'exact',
    valueType: filterBuilderValueTypes.BOOL,
  },
  {
    name: 'qualityProfileId',
    label: () => translate('QualityProfile'),
    type: 'exact',
    valueType: filterBuilderValueTypes.QUALITY_PROFILE,
  },
  {
    name: 'seriesType',
    label: () => translate('SeriesType'),
    type: 'exact',
    valueType: filterBuilderValueTypes.SERIES_TYPES,
  },
  {
    name: 'tags',
    label: () => translate('Tags'),
    type: 'array',
    valueType: filterBuilderValueTypes.TAG,
  },
];

interface StatisticsQueryParams {
  rootFolderPaths?: string[];
  rootFolderPathsNot?: boolean;
  tagIds?: number[];
  tagIdsNot?: boolean;
  qualityProfileIds?: number[];
  qualityProfileIdsNot?: boolean;
  monitored?: boolean;
  seriesTypes?: string[];
  seriesTypesNot?: boolean;
}

const mapFiltersToQueryParams = (
  filters: PropertyFilter[]
): StatisticsQueryParams => {
  const params: StatisticsQueryParams = {};

  filters.forEach((propertyFilter) => {
    const values = (
      Array.isArray(propertyFilter.value)
        ? propertyFilter.value
        : [propertyFilter.value]
    ).filter((value) => value != null);

    const isNegated =
      propertyFilter.type === filterTypes.NOT_EQUAL ||
      propertyFilter.type === filterTypes.NOT_CONTAINS;

    if (!values.length) {
      return;
    }

    switch (propertyFilter.key) {
      case 'rootFolderPath':
        params.rootFolderPaths = values.map(String);
        params.rootFolderPathsNot = isNegated || undefined;
        break;
      case 'tags':
        params.tagIds = values.map(Number);
        params.tagIdsNot = isNegated || undefined;
        break;
      case 'qualityProfileId':
        params.qualityProfileIds = values.map(Number);
        params.qualityProfileIdsNot = isNegated || undefined;
        break;
      case 'monitored':
        params.monitored = isNegated ? !values[0] : Boolean(values[0]);
        break;
      case 'seriesType':
        params.seriesTypes = values.map(String);
        params.seriesTypesNot = isNegated || undefined;
        break;
      default:
        break;
    }
  });

  return params;
};

const useStatistics = () => {
  const selectedFilterKey = useStatisticsOption('selectedFilterKey');
  const customFilters = useCustomFiltersList('statistics');

  const queryParams = useMemo(() => {
    return mapFiltersToQueryParams(
      findSelectedFilters(selectedFilterKey, FILTERS, customFilters)
    );
  }, [selectedFilterKey, customFilters]);

  const hasParams = Object.values(queryParams).some((value) => value != null);

  return useApiQuery<Statistics>({
    path: '/statistics',
    queryParams: hasParams ? (queryParams as QueryParams) : undefined,
    queryOptions: {
      placeholderData: keepPreviousData,
    },
  });
};

export default useStatistics;
