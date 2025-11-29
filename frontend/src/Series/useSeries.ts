import { useQueryClient } from '@tanstack/react-query';
import moment from 'moment';
import { useCallback, useMemo } from 'react';
import { FilterBuilderTag } from 'Components/Filter/Builder/FilterBuilderRowValue';
import { Filter, FilterBuilderProp } from 'Filters/Filter';
import { useCustomFiltersList } from 'Filters/useCustomFilters';
import useApiMutation from 'Helpers/Hooks/useApiMutation';
import useApiQuery from 'Helpers/Hooks/useApiQuery';
import {
  filterBuilderTypes,
  filterBuilderValueTypes,
  sortDirections,
} from 'Helpers/Props';
import { FilterType } from 'Helpers/Props/filterTypes';
import getFilterTypePredicate from 'Helpers/Props/getFilterTypePredicate';
import { SortDirection } from 'Helpers/Props/sortDirections';
import sortByProp from 'Utilities/Array/sortByProp';
import clientSideFilterAndSort from 'Utilities/Filter/clientSideFilterAndSort';
import translate from 'Utilities/String/translate';
import Series from './Series';
import { useSeriesOptions } from './seriesOptionsStore';

// Date filter predicate helper
const dateFilterPredicate = (
  itemDate: string | undefined,
  filterValue: string | Date,
  type: FilterType
): boolean => {
  if (!itemDate) return false;
  const predicate = getFilterTypePredicate(type);
  return predicate(itemDate, filterValue);
};

export const FILTERS: Filter[] = [
  {
    key: 'all',
    label: () => translate('All'),
    filters: [],
  },
  {
    key: 'monitored',
    label: () => translate('MonitoredOnly'),
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
    label: () => translate('UnmonitoredOnly'),
    filters: [
      {
        key: 'monitored',
        value: [false],
        type: 'equal',
      },
    ],
  },
  {
    key: 'continuing',
    label: () => translate('ContinuingOnly'),
    filters: [
      {
        key: 'status',
        value: 'continuing',
        type: 'equal',
      },
    ],
  },
  {
    key: 'ended',
    label: () => translate('EndedOnly'),
    filters: [
      {
        key: 'status',
        value: 'ended',
        type: 'equal',
      },
    ],
  },
  {
    key: 'missing',
    label: () => translate('MissingEpisodes'),
    filters: [
      {
        key: 'missing',
        value: [true],
        type: 'equal',
      },
    ],
  },
];

const SORT_PREDICATES = {
  status: (item: Series, _direction: SortDirection) => {
    let result = 0;

    if (item.monitored) {
      result += 2;
    }

    if (item.status === 'continuing') {
      result++;
    }

    return result;
  },

  sizeOnDisk: (item: Series, _direction: SortDirection) => {
    return item.statistics?.sizeOnDisk ?? 0;
  },

  network: (item: Series, _direction: SortDirection) => {
    const network = item.network;

    return network ? network.toLowerCase() : '';
  },

  nextAiring: (item: Series, direction: SortDirection) => {
    const nextAiring = item.nextAiring;

    if (nextAiring) {
      return moment(nextAiring).unix();
    }

    if (direction === sortDirections.DESCENDING) {
      return 0;
    }

    return Number.MAX_VALUE;
  },

  previousAiring: (item: Series, direction: SortDirection) => {
    const previousAiring = item.previousAiring;

    if (previousAiring) {
      return moment(previousAiring).unix();
    }

    if (direction === sortDirections.DESCENDING) {
      return -Number.MAX_VALUE;
    }

    return Number.MAX_VALUE;
  },

  episodeProgress: (item: Series, _direction: SortDirection) => {
    const statistics = item.statistics;

    const episodeCount = statistics?.episodeCount ?? 0;
    const episodeFileCount = statistics?.episodeFileCount ?? 0;

    const progress = episodeCount
      ? (episodeFileCount / episodeCount) * 100
      : 100;

    return progress + episodeCount / 1000000;
  },

  episodeCount: (item: Series, _direction: SortDirection) => {
    return item.statistics?.totalEpisodeCount ?? 0;
  },

  seasonCount: (item: Series, _direction: SortDirection) => {
    return item.statistics?.seasonCount ?? 0;
  },

  originalLanguage: (item: Series, _direction: SortDirection) => {
    const { originalLanguage } = item;

    return originalLanguage?.name ?? '';
  },

  ratings: (item: Series, _direction: SortDirection) => {
    const { ratings } = item;

    return ratings.value ?? 0;
  },

  monitorNewItems: (item: Series, _direction: SortDirection) => {
    return item.monitorNewItems === 'all' ? 1 : 0;
  },
} as const;

const FILTER_PREDICATES = {
  episodeProgress: (item: Series, filterValue: number, type: FilterType) => {
    const statistics = item.statistics;
    const episodeCount = statistics?.episodeCount ?? 0;
    const episodeFileCount = statistics?.episodeFileCount ?? 0;

    const progress = episodeCount
      ? (episodeFileCount / episodeCount) * 100
      : 100;

    const predicate = getFilterTypePredicate(type);
    return predicate(progress, filterValue);
  },

  missing: (item: Series, _filterValue: boolean, _type: FilterType) => {
    const statistics = item.statistics;
    const episodeCount = statistics?.episodeCount ?? 0;
    const episodeFileCount = statistics?.episodeFileCount ?? 0;
    return episodeCount - episodeFileCount > 0;
  },

  nextAiring: (item: Series, filterValue: string | Date, type: FilterType) => {
    return dateFilterPredicate(item.nextAiring, filterValue, type);
  },

  previousAiring: (
    item: Series,
    filterValue: string | Date,
    type: FilterType
  ) => {
    return dateFilterPredicate(item.previousAiring, filterValue, type);
  },

  added: (item: Series, filterValue: string | Date, type: FilterType) => {
    return dateFilterPredicate(item.added, filterValue, type);
  },

  ratings: (item: Series, filterValue: number, type: FilterType) => {
    const predicate = getFilterTypePredicate(type);
    const value = item.ratings.value ?? 0;
    return predicate(value * 10, filterValue);
  },

  ratingVotes: (item: Series, filterValue: number, type: FilterType) => {
    const predicate = getFilterTypePredicate(type);
    const votes = item.ratings.votes ?? 0;
    return predicate(votes, filterValue);
  },

  originalLanguage: (item: Series, filterValue: string, type: FilterType) => {
    const predicate = getFilterTypePredicate(type);
    const languageName = item.originalLanguage?.name ?? '';
    return predicate(languageName, filterValue);
  },

  releaseGroups: (item: Series, filterValue: string[], type: FilterType) => {
    const releaseGroups = item.statistics?.releaseGroups ?? [];
    const predicate = getFilterTypePredicate(type);
    return predicate(releaseGroups, filterValue);
  },

  seasonCount: (item: Series, filterValue: number, type: FilterType) => {
    const predicate = getFilterTypePredicate(type);
    const seasonCount = item.statistics?.seasonCount ?? 0;
    return predicate(seasonCount, filterValue);
  },

  sizeOnDisk: (item: Series, filterValue: number, type: FilterType) => {
    const predicate = getFilterTypePredicate(type);
    const sizeOnDisk = item.statistics?.sizeOnDisk ?? 0;
    return predicate(sizeOnDisk, filterValue);
  },

  hasMissingSeason: (item: Series, filterValue: boolean, type: FilterType) => {
    const predicate = getFilterTypePredicate(type);
    const seasons = item.seasons ?? [];

    const hasMissingSeason = seasons.some((season) => {
      const { seasonNumber } = season;
      const statistics = season.statistics;
      const episodeFileCount = statistics?.episodeFileCount ?? 0;
      const episodeCount = statistics?.episodeCount ?? 0;
      const totalEpisodeCount = statistics?.totalEpisodeCount ?? 0;

      return (
        seasonNumber > 0 &&
        totalEpisodeCount > 0 &&
        episodeCount === totalEpisodeCount &&
        episodeFileCount === 0
      );
    });

    return predicate(hasMissingSeason, filterValue);
  },

  seasonsMonitoredStatus: (
    item: Series,
    filterValue: string,
    type: FilterType
  ) => {
    const predicate = getFilterTypePredicate(type);
    const seasons = item.seasons ?? [];

    const { monitoredCount, unmonitoredCount } = seasons.reduce(
      (acc, { seasonNumber, monitored }) => {
        if (seasonNumber <= 0) {
          return acc;
        }

        if (monitored) {
          acc.monitoredCount++;
        } else {
          acc.unmonitoredCount++;
        }

        return acc;
      },
      { monitoredCount: 0, unmonitoredCount: 0 }
    );

    let seasonsMonitoredStatus = 'partial';

    if (monitoredCount === 0) {
      seasonsMonitoredStatus = 'none';
    } else if (unmonitoredCount === 0) {
      seasonsMonitoredStatus = 'all';
    }

    return predicate(seasonsMonitoredStatus, filterValue);
  },
} as const;

export const FILTER_BUILDER: FilterBuilderProp<Series>[] = [
  {
    name: 'monitored',
    label: () => translate('Monitored'),
    type: filterBuilderTypes.EXACT,
    valueType: filterBuilderValueTypes.BOOL,
  },
  {
    name: 'status',
    label: () => translate('Status'),
    type: filterBuilderTypes.EXACT,
    valueType: filterBuilderValueTypes.SERIES_STATUS,
  },
  {
    name: 'seriesType',
    label: () => translate('Type'),
    type: filterBuilderTypes.EXACT,
    valueType: filterBuilderValueTypes.SERIES_TYPES,
  },
  {
    name: 'title',
    label: () => translate('Title'),
    type: filterBuilderTypes.STRING,
  },
  {
    name: 'network',
    label: () => translate('Network'),
    type: filterBuilderTypes.ARRAY,
    optionsSelector: function (items: Series[]) {
      const tagList = items.reduce<FilterBuilderTag<string, string>[]>(
        (acc, series) => {
          if (series.network) {
            acc.push({
              id: series.network,
              name: series.network,
            });
          }

          return acc;
        },
        []
      );

      return tagList.sort(sortByProp('name'));
    },
  },
  {
    name: 'qualityProfileId',
    label: () => translate('QualityProfile'),
    type: filterBuilderTypes.EXACT,
    valueType: filterBuilderValueTypes.QUALITY_PROFILE,
  },
  {
    name: 'nextAiring',
    label: () => translate('NextAiring'),
    type: filterBuilderTypes.DATE,
    valueType: filterBuilderValueTypes.DATE,
  },
  {
    name: 'previousAiring',
    label: () => translate('PreviousAiring'),
    type: filterBuilderTypes.DATE,
    valueType: filterBuilderValueTypes.DATE,
  },
  {
    name: 'added',
    label: () => translate('Added'),
    type: filterBuilderTypes.DATE,
    valueType: filterBuilderValueTypes.DATE,
  },
  {
    name: 'seasonCount',
    label: () => translate('SeasonCount'),
    type: filterBuilderTypes.NUMBER,
  },
  {
    name: 'episodeProgress',
    label: () => translate('EpisodeProgress'),
    type: filterBuilderTypes.NUMBER,
  },
  {
    name: 'path',
    label: () => translate('Path'),
    type: filterBuilderTypes.STRING,
  },
  {
    name: 'rootFolderPath',
    label: () => translate('RootFolderPath'),
    type: filterBuilderTypes.EXACT,
  },
  {
    name: 'sizeOnDisk',
    label: () => translate('SizeOnDisk'),
    type: filterBuilderTypes.NUMBER,
    valueType: filterBuilderValueTypes.BYTES,
  },
  {
    name: 'genres',
    label: () => translate('Genres'),
    type: filterBuilderTypes.ARRAY,
    optionsSelector: function (items: Series[]) {
      const tagList = items.reduce<FilterBuilderTag<string, string>[]>(
        (acc, series) => {
          series.genres.forEach((genre) => {
            acc.push({
              id: genre,
              name: genre,
            });
          });

          return acc;
        },
        []
      );

      return tagList.sort(sortByProp('name'));
    },
  },
  {
    name: 'originalLanguage',
    label: () => translate('OriginalLanguage'),
    type: filterBuilderTypes.EXACT,
    optionsSelector: function (items: Series[]) {
      const languageList = items.reduce<FilterBuilderTag<string, string>[]>(
        (acc, series) => {
          if (series.originalLanguage) {
            acc.push({
              id: series.originalLanguage.name,
              name: series.originalLanguage.name,
            });
          }

          return acc;
        },
        []
      );

      return languageList.sort(sortByProp('name'));
    },
  },
  {
    name: 'releaseGroups',
    label: () => translate('ReleaseGroups'),
    type: filterBuilderTypes.ARRAY,
  },
  {
    name: 'ratings',
    label: () => translate('Rating'),
    type: filterBuilderTypes.NUMBER,
  },
  {
    name: 'ratingVotes',
    label: () => translate('RatingVotes'),
    type: filterBuilderTypes.NUMBER,
  },
  {
    name: 'certification',
    label: () => translate('Certification'),
    type: filterBuilderTypes.EXACT,
  },
  {
    name: 'tags',
    label: () => translate('Tags'),
    type: filterBuilderTypes.ARRAY,
    valueType: filterBuilderValueTypes.TAG,
  },
  {
    name: 'useSceneNumbering',
    label: () => translate('SceneNumbering'),
    type: filterBuilderTypes.EXACT,
  },
  {
    name: 'hasMissingSeason',
    label: () => translate('HasMissingSeason'),
    type: filterBuilderTypes.EXACT,
    valueType: filterBuilderValueTypes.BOOL,
  },
  {
    name: 'seasonsMonitoredStatus',
    label: () => translate('SeasonsMonitoredStatus'),
    type: filterBuilderTypes.EXACT,
    valueType: filterBuilderValueTypes.SEASONS_MONITORED_STATUS,
  },
  {
    name: 'year',
    label: () => translate('Year'),
    type: filterBuilderTypes.NUMBER,
  },
];

const DEFAULT_SERIES: Series[] = [];

const useSeries = () => {
  const { data, ...result } = useApiQuery<Series[]>({
    path: '/series',
    queryOptions: {
      staleTime: 5 * 60 * 1000,
      gcTime: Infinity,
    },
  });

  const seriesMap = useMemo(() => {
    if (!data) {
      return new Map<number, Series>();
    }

    return new Map<number, Series>(data.map((series) => [series.id, series]));
  }, [data]);

  return {
    ...result,
    data: data ?? DEFAULT_SERIES,
    seriesMap,
  };
};

export default useSeries;

export const useSeriesIndex = () => {
  const { selectedFilterKey, sortKey, sortDirection } = useSeriesOptions();
  const { data: seriesData = [], ...queryResult } = useSeries();
  const customFilters = useCustomFiltersList('series');

  const data = useMemo(() => {
    return clientSideFilterAndSort<
      Series,
      typeof FILTER_PREDICATES,
      typeof SORT_PREDICATES
    >(seriesData, {
      selectedFilterKey,
      filters: FILTERS,
      filterPredicates: FILTER_PREDICATES,
      customFilters,
      sortKey: sortKey as keyof Series,
      sortDirection,
      secondarySortKey: 'sortTitle',
      secondarySortDirection: 'ascending',
      sortPredicates: SORT_PREDICATES,
    });
  }, [customFilters, seriesData, selectedFilterKey, sortKey, sortDirection]);

  return {
    ...queryResult,
    data: data.data,
    totalItems: data.totalItems,
  };
};

export const useHasSeries = () => {
  const { data: seriesData = [] } = useSeries();

  return useMemo(() => {
    return seriesData.length > 0;
  }, [seriesData]);
};

export const useSingleSeries = (seriesId?: number) => {
  const { seriesMap } = useSeries();

  return useMemo(() => {
    if (!seriesId) {
      return undefined;
    }

    return seriesMap.get(seriesId);
  }, [seriesMap, seriesId]);
};

export const useMultipleSeries = (seriesIds: number[]) => {
  const { seriesMap } = useSeries();

  return useMemo(() => {
    if (seriesIds.length === 0) {
      return DEFAULT_SERIES;
    }

    return seriesIds.reduce((acc: Series[], seriesId) => {
      const series = seriesMap.get(seriesId);

      if (series) {
        acc.push(series);
      }

      return acc;
    }, []);
  }, [seriesMap, seriesIds]);
};

interface SaveSeriesPayload extends Partial<Series> {
  id: number;
}

interface DeleteSeriesPayload {
  deleteFiles?: boolean;
  addImportListExclusion?: boolean;
}

interface ToggleSeriesMonitoredPayload {
  monitored: boolean;
}

interface ToggleSeasonMonitoredPayload {
  seasonNumber: number;
  monitored: boolean;
}

interface UpdateSeriesMonitorPayload {
  series: {
    id: number;
    monitored?: boolean;
    seasons?: {
      seasonNumber: number;
      monitored: boolean;
    }[];
  }[];
  monitoringOptions?: {
    monitor: string;
  };
}

interface BulkDeleteSeriesPayload {
  seriesIds: number[];
  deleteFiles?: boolean;
  addImportListExclusion?: boolean;
}

interface SaveSeriesEditorPayload {
  seriesIds: number[];
  monitored?: boolean;
  qualityProfileId?: number;
  seriesType?: string;
  seasonFolder?: boolean;
  rootFolderPath?: string;
  tags?: number[];
}

export const useSaveSeries = (moveFiles?: boolean) => {
  const queryClient = useQueryClient();

  const { mutate, isPending, error } = useApiMutation<
    Series,
    SaveSeriesPayload
  >({
    path: '/series',
    queryParams: {
      moveFiles,
    },
    method: 'PUT',
    mutationOptions: {
      onSuccess: (updatedSeries) => {
        queryClient.setQueryData<Series[]>(['/series'], (oldSeries) => {
          if (!oldSeries) {
            return oldSeries;
          }

          return oldSeries.map((series) => {
            if (series.id === updatedSeries.id) {
              return {
                ...series,
                ...updatedSeries,
              };
            }

            return series;
          });
        });
      },
    },
  });

  return {
    saveSeries: mutate,
    isSaving: isPending,
    saveError: error,
  };
};

export const useDeleteSeries = (
  seriesId: number,
  options: DeleteSeriesPayload
) => {
  const queryClient = useQueryClient();

  const { mutate, isPending, error } = useApiMutation<unknown, void>({
    path: `/series/${seriesId}`,
    queryParams: {
      ...options,
    },
    method: 'DELETE',
    mutationOptions: {
      onSuccess: () => {
        queryClient.setQueryData<Series[]>(['/series'], (oldSeries) => {
          if (!oldSeries) {
            return oldSeries;
          }

          return oldSeries.filter((series) => series.id !== seriesId);
        });
      },
    },
  });

  return {
    deleteSeries: mutate,
    isDeleting: isPending,
    deleteError: error,
  };
};

export const useToggleSeriesMonitored = (seriesId: number) => {
  const queryClient = useQueryClient();
  const series = useSingleSeries(seriesId);

  const { mutate, isPending, error } = useApiMutation<
    Series,
    ToggleSeriesMonitoredPayload
  >({
    path: '/series',
    method: 'PUT',
    mutationOptions: {
      onSuccess: (updatedSeries) => {
        queryClient.setQueryData<Series[]>(['/series'], (oldSeries) => {
          if (!oldSeries) {
            return oldSeries;
          }

          return oldSeries.map((series) =>
            series.id === updatedSeries.id ? updatedSeries : series
          );
        });
      },
    },
  });
  const toggleSeriesMonitored = useCallback(
    (payload: ToggleSeriesMonitoredPayload) => {
      return mutate({ ...series, ...payload });
    },
    [series, mutate]
  );

  return {
    toggleSeriesMonitored,
    isTogglingSeriesMonitored: isPending,
    toggleSeriesMonitoredError: error,
  };
};

export const useToggleSeasonMonitored = (seriesId: number) => {
  const queryClient = useQueryClient();

  const { mutate, isPending, error } = useApiMutation<
    Series,
    ToggleSeasonMonitoredPayload
  >({
    path: `/series/${seriesId}/season`,
    method: 'PUT',
    mutationOptions: {
      onSuccess: (updatedSeries) => {
        queryClient.setQueryData<Series[]>(['/series'], (oldSeries) => {
          if (!oldSeries) {
            return oldSeries;
          }

          return oldSeries.map((series) => {
            if (series.id === updatedSeries.id) {
              return {
                ...series,
                seasons: series.seasons.map((season) => {
                  const updatedSeason = updatedSeries.seasons.find(
                    (s) => s.seasonNumber === season.seasonNumber
                  );

                  if (updatedSeason) {
                    return {
                      ...season,
                      ...updatedSeason,
                    };
                  }

                  return season;
                }),
              };
            }

            return series;
          });
        });
      },
    },
  });

  return {
    toggleSeasonMonitored: mutate,
    isTogglingSeasonMonitored: isPending,
    toggleSeasonMonitoredError: error,
  };
};

export const useUpdateSeriesMonitor = (
  shouldFetchEpisodesAfterUpdate = false
) => {
  const queryClient = useQueryClient();

  const { mutate, isPending, error } = useApiMutation<
    void,
    UpdateSeriesMonitorPayload
  >({
    path: '/seasonPass',
    method: 'POST',
    mutationOptions: {
      onSuccess: (_, variables) => {
        if (shouldFetchEpisodesAfterUpdate) {
          queryClient.invalidateQueries({ queryKey: ['/episode'] });
        }

        queryClient.setQueryData<Series[]>(['/series'], (oldSeries) => {
          if (!oldSeries) {
            return oldSeries;
          }

          return oldSeries.map((series) => {
            const updatedSeries = variables.series.find(
              (s) => s.id === series.id
            );

            if (!updatedSeries) {
              return series;
            }

            return {
              ...series,
              monitored: updatedSeries.monitored ?? series.monitored,
              seasons: series.seasons.map((season) => {
                const updatedSeason = updatedSeries.seasons?.find(
                  (s) => s.seasonNumber === season.seasonNumber
                );

                if (updatedSeason) {
                  return {
                    ...season,
                    monitored: updatedSeason.monitored,
                  };
                }

                return season;
              }),
            };
          });
        });
      },
    },
  });

  return {
    updateSeriesMonitor: mutate,
    isUpdatingSeriesMonitor: isPending,
    updateSeriesMonitorError: error,
  };
};

export const useSaveSeriesEditor = () => {
  const queryClient = useQueryClient();

  const { mutate, isPending, error } = useApiMutation<
    Series[],
    SaveSeriesEditorPayload
  >({
    path: '/series/editor',
    method: 'PUT',
    mutationOptions: {
      onSuccess: (updatedSeries) => {
        queryClient.setQueryData<Series[]>(['/series'], (oldSeries) => {
          if (!oldSeries) {
            return oldSeries;
          }

          return oldSeries.map((series) => {
            const updatedSeriesData = updatedSeries.find(
              (updated) => updated.id === series.id
            );

            if (updatedSeriesData) {
              const {
                alternateTitles,
                images,
                rootFolderPath,
                statistics,
                ...propsToUpdate
              } = updatedSeriesData;

              return { ...series, ...propsToUpdate };
            }

            return series;
          });
        });
      },
    },
  });

  return {
    saveSeriesEditor: mutate,
    isSavingSeriesEditor: isPending,
    saveSeriesEditorError: error,
  };
};

export const useBulkDeleteSeries = () => {
  const queryClient = useQueryClient();

  const { mutate, isPending, error } = useApiMutation<
    void,
    BulkDeleteSeriesPayload
  >({
    path: '/series/editor',
    method: 'DELETE',
    mutationOptions: {
      onSuccess: (_, variables) => {
        const seriesIds = new Set(variables.seriesIds);

        queryClient.setQueryData<Series[]>(['/series'], (oldSeries) => {
          if (!oldSeries) {
            return oldSeries;
          }

          return oldSeries.filter((series) => !seriesIds.has(series.id));
        });
      },
    },
  });

  return {
    bulkDeleteSeries: mutate,
    isBulkDeleting: isPending,
    bulkDeleteError: error,
  };
};
