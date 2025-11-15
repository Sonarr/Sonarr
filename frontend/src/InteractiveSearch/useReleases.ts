import { useEffect, useMemo, useState } from 'react';
import { useSelector } from 'react-redux';
import { create } from 'zustand';
import ModelBase from 'App/ModelBase';
import { Filter, FilterBuilderProp } from 'App/State/AppState';
import { FilterBuilderTag } from 'Components/Filter/Builder/FilterBuilderRowValue';
import type DownloadProtocol from 'DownloadClient/DownloadProtocol';
import useApiMutation from 'Helpers/Hooks/useApiMutation';
import useApiQuery from 'Helpers/Hooks/useApiQuery';
import { applySort } from 'Helpers/Hooks/useOptionsStore';
import { filterBuilderTypes, filterBuilderValueTypes } from 'Helpers/Props';
import { FilterType } from 'Helpers/Props/filterTypes';
import getFilterTypePredicate from 'Helpers/Props/getFilterTypePredicate';
import { SortDirection } from 'Helpers/Props/sortDirections';
import Language from 'Language/Language';
import { QualityModel } from 'Quality/Quality';
import { AlternateTitle } from 'Series/Series';
import { createCustomFiltersSelector } from 'Store/Selectors/createClientSideCollectionSelector';
import CustomFormat from 'typings/CustomFormat';
import Rejection from 'typings/Rejection';
import sortByProp from 'Utilities/Array/sortByProp';
import clientSideFilterAndSort from 'Utilities/Filter/clientSideFilterAndSort';
import enumToTitle from 'Utilities/String/enumToTitle';
import translate from 'Utilities/String/translate';
import InteractiveSearchPayload from './InteractiveSearchPayload';
import {
  getReleaseOption,
  setReleaseOption,
  useReleaseOptions,
} from './releaseOptionsStore';

export interface ReleaseEpisode {
  id: number;
  episodeFileId: number;
  seasonNumber: number;
  episodeNumber: number;
  absoluteEpisodeNumber?: number;
  title: string;
}

export interface Release extends ModelBase {
  parsedInfo: ParsedInfo;
  release: ReleaseInfo;
  decision: Decision;
  qualityWeight: number;
  languages: Language[];
  mappedSeriesId?: number;
  mappedSeasonNumber?: number;
  mappedEpisodeNumbers?: number[];
  mappedAbsoluteEpisodeNumbers?: number[];
  mappedEpisodeInfo: ReleaseEpisode[];
  publishDate: string;
  episodeRequested: boolean;
  downloadAllowed: boolean;
  releaseWeight: number;
  customFormats: CustomFormat[];
  customFormatScore: number;
  indexerFlags: number;
  sceneMapping?: AlternateTitle;
}

export interface ParsedInfo {
  quality: QualityModel;
  releaseGroup: string;
  releaseHash: string;
  fullSeason: boolean;
  seasonNumber: number;
  seriesTitle: string;
  episodeNumbers: number[];
  absoluteEpisodeNumbers?: number[];
  isDaily: boolean;
  isAbsoluteNumbering: boolean;
  isPossibleSpecialEpisode: boolean;
  special: boolean;
}

export interface ReleaseInfo {
  guid: string;
  age: number;
  ageHours: number;
  ageMinutes: number;
  size: number;
  indexerId: number;
  indexer: string;
  title: string;
  tvdbId: number;
  tvRageId: number;
  publishDate: string;
  commentUrl: string;
  downloadUrl: string;
  infoUrl: string;
  protocol: DownloadProtocol;
  indexerFlags: number;
  seeders?: number;
  leechers?: number;
  magnetUrl?: string;
  infoHash?: string;
}

export interface Decision {
  approved: boolean;
  temporarilyRejected: boolean;
  rejected: boolean;
  rejections: Rejection[];
}

export const FILTERS: Filter[] = [
  {
    key: 'all',
    label: () => translate('All'),
    filters: [],
  },
  {
    key: 'season-pack',
    label: () => translate('SeasonPack'),
    filters: [
      {
        key: 'fullSeason',
        value: [true],
        type: 'equal',
      },
    ],
  },
  {
    key: 'not-season-pack',
    label: () => translate('NotSeasonPack'),
    filters: [
      {
        key: 'fullSeason',
        value: [false],
        type: 'equal',
      },
    ],
  },
];

export const FILTER_BUILDER: FilterBuilderProp<Release>[] = [
  {
    name: 'title',
    label: () => translate('Title'),
    type: filterBuilderTypes.STRING,
  },
  {
    name: 'age',
    label: () => translate('Age'),
    type: filterBuilderTypes.NUMBER,
  },
  {
    name: 'protocol',
    label: () => translate('Protocol'),
    type: filterBuilderTypes.EXACT,
    valueType: filterBuilderValueTypes.PROTOCOL,
  },
  {
    name: 'indexerId',
    label: () => translate('Indexer'),
    type: filterBuilderTypes.EXACT,
    valueType: filterBuilderValueTypes.INDEXER,
  },
  {
    name: 'size',
    label: () => translate('Size'),
    type: filterBuilderTypes.NUMBER,
    valueType: filterBuilderValueTypes.BYTES,
  },
  {
    name: 'seeders',
    label: () => translate('Seeders'),
    type: filterBuilderTypes.NUMBER,
  },
  {
    name: 'peers',
    label: () => translate('Peers'),
    type: filterBuilderTypes.NUMBER,
  },
  {
    name: 'quality',
    label: () => translate('Quality'),
    type: filterBuilderTypes.EXACT,
    valueType: filterBuilderValueTypes.QUALITY,
  },
  {
    name: 'languages',
    label: () => translate('Languages'),
    type: filterBuilderTypes.ARRAY,
    optionsSelector: function (items) {
      const languageList = items.reduce<FilterBuilderTag<string, string>[]>(
        (acc, release) => {
          release.languages.forEach((language) => {
            acc.push({
              id: language.name,
              name: language.name,
            });
          });

          return acc;
        },
        []
      );

      return languageList.sort(
        sortByProp<FilterBuilderTag<string, string>, 'name'>('name')
      );
    },
  },
  {
    name: 'customFormatScore',
    label: () => translate('CustomFormatScore'),
    type: filterBuilderTypes.NUMBER,
  },
  {
    name: 'rejectionCount',
    label: () => translate('RejectionCount'),
    type: filterBuilderTypes.NUMBER,
  },
  {
    name: 'rejections',
    label: () => translate('Rejections'),
    type: filterBuilderTypes.ARRAY,
    optionsSelector: function () {
      return getReleaseOption('rejectionFilterTags');
    },
  },
  {
    name: 'fullSeason',
    label: () => translate('SeasonPack'),
    type: filterBuilderTypes.EXACT,
    valueType: filterBuilderValueTypes.BOOL,
  },
  {
    name: 'episodeRequested',
    label: () => translate('EpisodeRequested'),
    type: filterBuilderTypes.EXACT,
    valueType: filterBuilderValueTypes.BOOL,
  },
];

const FILTER_PREDICATES = {
  age: (item: Release, value: number, type: FilterType) => {
    return applyFilterPredicate(item.release.age, value, type);
  },

  episodeRequested: (item: Release, value: boolean, type: FilterType) => {
    return applyFilterPredicate(item.episodeRequested, value, type);
  },

  fullSeason: (item: Release, value: boolean, type: FilterType) => {
    return applyFilterPredicate(item.parsedInfo.fullSeason, value, type);
  },

  indexerId: (item: Release, value: number, type: FilterType) => {
    return applyFilterPredicate(item.release.indexerId, value, type);
  },

  languages: (item: Release, filterValue: string[], type: FilterType) => {
    const languages = item.languages.map((language) => language.name);

    return applyFilterPredicate(languages, filterValue, type);
  },

  peers: (item: Release, value: number, type: FilterType) => {
    const seeders = item.release.seeders || 0;
    const leechers = item.release.leechers || 0;
    const peers = seeders + leechers;

    return applyFilterPredicate(peers, value, type);
  },

  protocol: (item: Release, value: DownloadProtocol, type: FilterType) => {
    return applyFilterPredicate(item.release.protocol, value, type);
  },

  quality: (item: Release, value: number, type: FilterType) => {
    return applyFilterPredicate(
      item.parsedInfo.quality.quality.id,
      value,
      type
    );
  },

  rejectionCount: (item: Release, value: number, type: FilterType) => {
    return applyFilterPredicate(item.decision.rejections.length, value, type);
  },

  rejections: (item: Release, value: string[], type: FilterType) => {
    return applyFilterPredicate(
      item.decision.rejections.map((r) => r.reason),
      value,
      type
    );
  },

  seeders: (item: Release, value: number, type: FilterType) => {
    return applyFilterPredicate(item.release.seeders ?? 0, value, type);
  },

  size: (item: Release, value: number, type: FilterType) => {
    return applyFilterPredicate(item.release.size, value, type);
  },

  title: (item: Release, value: string, type: FilterType) => {
    return applyFilterPredicate(item.release.title, value, type);
  },
} as const;

const SORT_PREDICATES = {
  age: function (item: Release, _direction: SortDirection) {
    return item.release.ageMinutes;
  },

  indexer: (item: Release, _direction: SortDirection) => {
    return item.release.indexerId;
  },

  indexerFlags: (item: Release, _direction: SortDirection) => {
    return item.release.indexerFlags;
  },

  languages: (item: Release, _direction: SortDirection) => {
    if (item.languages.length > 1) {
      return 10000;
    }

    return item.languages[0]?.id ?? 0;
  },

  peers: (item: Release, _direction: SortDirection) => {
    const seeders = item.release.seeders || 0;
    const leechers = item.release.leechers || 0;

    return seeders * 1000000 + leechers;
  },

  protocol: (item: Release, _direction: SortDirection) => {
    return item.release.protocol;
  },

  qualityWeight: (item: Release, _direction: SortDirection) => {
    return item.qualityWeight;
  },

  rejections: (item: Release, _direction: SortDirection) => {
    const rejections = item.decision.rejections;
    const releaseWeight = item.releaseWeight;

    if (rejections.length !== 0) {
      return releaseWeight + 1000000;
    }

    return releaseWeight;
  },

  size: (item: Release, _direction: SortDirection) => {
    return item.release.size;
  },

  title: (item: Release, _direction: SortDirection) => {
    return item.release.title;
  },
} as const;

interface ReleaseStore {
  sortKey: string;
  sortDirection: SortDirection;
}

const releaseStore = create<ReleaseStore>(() => ({
  sortKey: 'releaseWeight',
  sortDirection: 'ascending',
}));

const DEFAULT_RELEASES: Release[] = [];
const THIRTY_MINUTES = 30 * 60 * 1000;

const useReleases = (payload: InteractiveSearchPayload) => {
  const customFilters = useSelector(createCustomFiltersSelector('releases'));
  const { episodeSelectedFilterKey, seasonSelectedFilterKey } =
    useReleaseOptions();

  const { sortKey, sortDirection } = releaseStore();

  const selectedFilterKey =
    'seriesId' in payload ? seasonSelectedFilterKey : episodeSelectedFilterKey;

  const { data, queryKey, ...result } = useApiQuery<Release[]>({
    path: '/release',
    queryParams: {
      ...payload,
    },
    queryOptions: {
      // Cache and stale times set to 30 minutes
      staleTime: THIRTY_MINUTES,
      gcTime: THIRTY_MINUTES,
      refetchOnMount: 'always',
      // Disable refetch on window focus to prevent refetching when the user switch tabs
      refetchOnWindowFocus: false,
      retry: false,
    },
  });

  const { data: filteredData, totalItems } = useMemo(
    () =>
      clientSideFilterAndSort<Release, typeof FILTER_PREDICATES>(
        data ?? DEFAULT_RELEASES,
        {
          selectedFilterKey,
          filters: FILTERS,
          filterPredicates: FILTER_PREDICATES,
          customFilters,
          sortKey,
          sortDirection,
          sortPredicates: SORT_PREDICATES,
        }
      ),
    [data, selectedFilterKey, customFilters, sortKey, sortDirection]
  );

  useEffect(() => {
    if (!data) {
      return;
    }

    // Get existing rejection tags as a map for easy lookup
    const rejectionsMap = new Map(
      getReleaseOption('rejectionFilterTags').map((tag) => [tag.id, tag])
    );

    data.forEach((release) => {
      release.decision.rejections.forEach((rejection) => {
        if (!rejectionsMap.has(rejection.reason)) {
          rejectionsMap.set(rejection.reason, {
            id: rejection.reason,
            name: enumToTitle(rejection.reason),
          });
        }
      });
    });

    const rejections = Array.from(rejectionsMap.values()).sort(
      sortByProp<FilterBuilderTag<string, string>, 'name'>('name')
    );

    setReleaseOption('rejectionFilterTags', rejections);
  }, [data]);

  return {
    ...result,
    data: filteredData,
    selectedFilterKey,
    sortKey,
    sortDirection,
    totalItems,
  };
};

export default useReleases;

interface OverrideRelease {
  seriesId: number;
  episodeIds: number[];
  downloadClientId: number | null;
  quality: QualityModel;
  languages: Language[];
}

interface GrabRelease {
  guid: string;
  indexerId: number;
  override?: OverrideRelease;
  searchInfo?: InteractiveSearchPayload;
}

export const useGrabRelease = () => {
  const [isGrabbed, setIsGrabbed] = useState(false);

  // Explicitly define the types for the mutation so we can pass in no arguments to mutate as expected.
  const { mutate, isPending, error } = useApiMutation<unknown, GrabRelease>({
    path: '/release',
    method: 'POST',
    mutationOptions: {
      onMutate: () => {
        setIsGrabbed(false);
      },
      onSuccess: () => {
        setIsGrabbed(true);
      },
    },
  });

  const grabError = useMemo(() => {
    if (!error) {
      return undefined;
    }

    return error.statusBody?.message ?? translate('InteractiveSearchGrabError');
  }, [error]);

  return {
    grabRelease: mutate,
    isGrabbing: isPending,
    isGrabbed,
    grabError,
  };
};

export const setReleaseSort = (
  sortKey: string,
  sortDirection: SortDirection | undefined
) => {
  releaseStore.setState((state) => applySort(state, sortKey, sortDirection));
};

const applyFilterPredicate = <T>(itemValue: T, value: T, type: FilterType) => {
  const predicate = getFilterTypePredicate(type);

  return predicate(itemValue, value);
};
