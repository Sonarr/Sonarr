import Column from 'Components/Table/Column';
import { createOptionsStore } from 'Helpers/Hooks/useOptionsStore';
import translate from 'Utilities/String/translate';

export interface SeriesOptions {
  selectedFilterKey: string | number;
  sortKey: string;
  sortDirection: 'ascending' | 'descending';
  view: string;
  columns: Column[];
  posterOptions: {
    detailedProgressBar: boolean;
    size: 'small' | 'medium' | 'large';
    showTitle: boolean;
    showMonitored: boolean;
    showQualityProfile: boolean;
    showTags: boolean;
    showSearchAction: boolean;
  };
  overviewOptions: {
    detailedProgressBar: boolean;
    size: 'small' | 'medium' | 'large';
    showMonitored: boolean;
    showNetwork: boolean;
    showQualityProfile: boolean;
    showPreviousAiring: boolean;
    showAdded: boolean;
    showSeasonCount: boolean;
    showPath: boolean;
    showSizeOnDisk: boolean;
    showTags: boolean;
    showSearchAction: boolean;
  };
  tableOptions: {
    showBanners: boolean;
    showSearchAction: boolean;
  };
  deleteOptions: {
    addImportListExclusion: boolean;
  };
}

const { useOptions, useOption, setOptions, setOption, setSort, getOptions } =
  createOptionsStore<SeriesOptions>('series_options', () => {
    return {
      selectedFilterKey: 'all',
      sortKey: 'sortTitle',
      sortDirection: 'ascending',
      secondarySortKey: 'sortTitle',
      secondarySortDirection: 'ascending',
      view: 'posters',
      posterOptions: {
        detailedProgressBar: false,
        size: 'large',
        showTitle: false,
        showMonitored: true,
        showQualityProfile: true,
        showTags: false,
        showSearchAction: false,
      },
      overviewOptions: {
        detailedProgressBar: false,
        size: 'medium',
        showMonitored: true,
        showNetwork: true,
        showQualityProfile: true,
        showPreviousAiring: false,
        showAdded: false,
        showSeasonCount: true,
        showPath: false,
        showSizeOnDisk: false,
        showTags: false,
        showSearchAction: false,
      },
      tableOptions: {
        showBanners: false,
        showSearchAction: false,
      },
      deleteOptions: {
        addImportListExclusion: false,
      },
      columns: [
        {
          name: 'status',
          label: '',
          columnLabel: () => translate('Status'),
          isSortable: true,
          isVisible: true,
          isModifiable: false,
        },
        {
          name: 'sortTitle',
          label: () => translate('SeriesTitle'),
          isSortable: true,
          isVisible: true,
          isModifiable: false,
        },
        {
          name: 'seriesType',
          label: () => translate('Type'),
          isSortable: true,
          isVisible: false,
        },
        {
          name: 'network',
          label: () => translate('Network'),
          isSortable: true,
          isVisible: true,
        },
        {
          name: 'qualityProfileId',
          label: () => translate('QualityProfile'),
          isSortable: true,
          isVisible: true,
        },
        {
          name: 'nextAiring',
          label: () => translate('NextAiring'),
          isSortable: true,
          isVisible: true,
        },
        {
          name: 'previousAiring',
          label: () => translate('PreviousAiring'),
          isSortable: true,
          isVisible: false,
        },
        {
          name: 'originalLanguage',
          label: () => translate('OriginalLanguage'),
          isSortable: true,
          isVisible: false,
        },
        {
          name: 'added',
          label: () => translate('Added'),
          isSortable: true,
          isVisible: false,
        },
        {
          name: 'seasonCount',
          label: () => translate('Seasons'),
          isSortable: true,
          isVisible: true,
        },
        {
          name: 'seasonFolder',
          label: () => translate('SeasonFolder'),
          isSortable: true,
          isVisible: false,
        },
        {
          name: 'episodeProgress',
          label: () => translate('Episodes'),
          isSortable: true,
          isVisible: true,
        },
        {
          name: 'episodeCount',
          label: () => translate('EpisodeCount'),
          isSortable: true,
          isVisible: false,
        },
        {
          name: 'latestSeason',
          label: () => translate('LatestSeason'),
          isSortable: true,
          isVisible: false,
        },
        {
          name: 'year',
          label: () => translate('Year'),
          isSortable: true,
          isVisible: false,
        },
        {
          name: 'path',
          label: () => translate('Path'),
          isSortable: true,
          isVisible: false,
        },
        {
          name: 'sizeOnDisk',
          label: () => translate('SizeOnDisk'),
          isSortable: true,
          isVisible: false,
        },
        {
          name: 'genres',
          label: () => translate('Genres'),
          isSortable: false,
          isVisible: false,
        },
        {
          name: 'ratings',
          label: () => translate('Rating'),
          isSortable: true,
          isVisible: false,
        },
        {
          name: 'certification',
          label: () => translate('Certification'),
          isSortable: false,
          isVisible: false,
        },
        {
          name: 'releaseGroups',
          label: () => translate('ReleaseGroups'),
          isSortable: false,
          isVisible: false,
        },
        {
          name: 'tags',
          label: () => translate('Tags'),
          isSortable: true,
          isVisible: false,
        },
        {
          name: 'useSceneNumbering',
          label: () => translate('SceneNumbering'),
          isSortable: true,
          isVisible: false,
        },
        {
          name: 'monitorNewItems',
          label: () => translate('MonitorNewSeasons'),
          isSortable: true,
          isVisible: false,
        },
        {
          name: 'actions',
          label: '',
          columnLabel: () => translate('Actions'),
          isVisible: true,
          isModifiable: false,
        },
      ],
    };
  });

export const useSeriesOptions = useOptions;
export const setSeriesOptions = setOptions;
export const useSeriesOption = useOption;
export const setSeriesOption = setOption;
export const setSeriesSort = setSort;

export const useSeriesPosterOptions = () => useOption('posterOptions');
export const setSeriesPosterOptions = (
  options: Partial<SeriesOptions['posterOptions']>
) => {
  const currentOptions = getOptions().posterOptions;
  setSeriesOption('posterOptions', { ...currentOptions, ...options });
};

export const useSeriesOverviewOptions = () => useOption('overviewOptions');
export const setSeriesOverviewOptions = (
  options: Partial<SeriesOptions['overviewOptions']>
) => {
  const currentOptions = getOptions().overviewOptions;
  setSeriesOption('overviewOptions', { ...currentOptions, ...options });
};

export const useSeriesTableOptions = () => useOption('tableOptions');
export const setSeriesTableOptions = (
  options: Partial<SeriesOptions['tableOptions']>
) => {
  const currentOptions = getOptions().tableOptions;
  setSeriesOption('tableOptions', { ...currentOptions, ...options });
};

export const useSeriesDeleteOptions = () => useOption('deleteOptions');
export const setSeriesDeleteOptions = (
  options: Partial<SeriesOptions['deleteOptions']>
) => {
  const currentOptions = getOptions().deleteOptions;
  setSeriesOption('deleteOptions', { ...currentOptions, ...options });
};
