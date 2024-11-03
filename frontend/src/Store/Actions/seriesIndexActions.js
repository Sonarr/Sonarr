import moment from 'moment';
import { createAction } from 'redux-actions';
import { sortDirections } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import createHandleActions from './Creators/createHandleActions';
import createSetClientSideCollectionFilterReducer from './Creators/Reducers/createSetClientSideCollectionFilterReducer';
import createSetClientSideCollectionSortReducer from './Creators/Reducers/createSetClientSideCollectionSortReducer';
import createSetTableOptionReducer from './Creators/Reducers/createSetTableOptionReducer';
import { filterBuilderProps, filterPredicates, filters, sortPredicates } from './seriesActions';

//
// Variables

export const section = 'seriesIndex';

//
// State

export const defaultState = {
  sortKey: 'sortTitle',
  sortDirection: sortDirections.ASCENDING,
  secondarySortKey: 'sortTitle',
  secondarySortDirection: sortDirections.ASCENDING,
  view: 'posters',

  posterOptions: {
    detailedProgressBar: false,
    size: 'large',
    showTitle: false,
    showMonitored: true,
    showQualityProfile: true,
    showTags: false,
    showSearchAction: false
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
    showSearchAction: false
  },

  tableOptions: {
    showBanners: false,
    showSearchAction: false
  },

  columns: [
    {
      name: 'status',
      columnLabel: () => translate('Status'),
      isSortable: true,
      isVisible: true,
      isModifiable: false
    },
    {
      name: 'sortTitle',
      label: () => translate('SeriesTitle'),
      isSortable: true,
      isVisible: true,
      isModifiable: false
    },
    {
      name: 'seriesType',
      label: () => translate('Type'),
      isSortable: true,
      isVisible: false
    },
    {
      name: 'network',
      label: () => translate('Network'),
      isSortable: true,
      isVisible: true
    },
    {
      name: 'qualityProfileId',
      label: () => translate('QualityProfile'),
      isSortable: true,
      isVisible: true
    },
    {
      name: 'nextAiring',
      label: () => translate('NextAiring'),
      isSortable: true,
      isVisible: true
    },
    {
      name: 'previousAiring',
      label: () => translate('PreviousAiring'),
      isSortable: true,
      isVisible: false
    },
    {
      name: 'originalLanguage',
      label: () => translate('OriginalLanguage'),
      isSortable: true,
      isVisible: false
    },
    {
      name: 'added',
      label: () => translate('Added'),
      isSortable: true,
      isVisible: false
    },
    {
      name: 'seasonCount',
      label: () => translate('Seasons'),
      isSortable: true,
      isVisible: true
    },
    {
      name: 'seasonFolder',
      label: () => translate('SeasonFolder'),
      isSortable: true,
      isVisible: false
    },
    {
      name: 'episodeProgress',
      label: () => translate('Episodes'),
      isSortable: true,
      isVisible: true
    },
    {
      name: 'episodeCount',
      label: () => translate('EpisodeCount'),
      isSortable: true,
      isVisible: false
    },
    {
      name: 'latestSeason',
      label: () => translate('LatestSeason'),
      isSortable: true,
      isVisible: false
    },
    {
      name: 'year',
      label: () => translate('Year'),
      isSortable: true,
      isVisible: false
    },
    {
      name: 'path',
      label: () => translate('Path'),
      isSortable: true,
      isVisible: false
    },
    {
      name: 'sizeOnDisk',
      label: () => translate('SizeOnDisk'),
      isSortable: true,
      isVisible: false
    },
    {
      name: 'genres',
      label: () => translate('Genres'),
      isSortable: false,
      isVisible: false
    },
    {
      name: 'ratings',
      label: () => translate('Rating'),
      isSortable: true,
      isVisible: false
    },
    {
      name: 'certification',
      label: () => translate('Certification'),
      isSortable: false,
      isVisible: false
    },
    {
      name: 'releaseGroups',
      label: () => translate('ReleaseGroups'),
      isSortable: false,
      isVisible: false
    },
    {
      name: 'tags',
      label: () => translate('Tags'),
      isSortable: true,
      isVisible: false
    },
    {
      name: 'useSceneNumbering',
      label: () => translate('SceneNumbering'),
      isSortable: true,
      isVisible: false
    },
    {
      name: 'monitorNewItems',
      label: () => translate('MonitorNewSeasons'),
      isSortable: true,
      isVisible: false
    },
    {
      name: 'actions',
      columnLabel: () => translate('Actions'),
      isVisible: true,
      isModifiable: false
    }
  ],

  sortPredicates: {
    ...sortPredicates,

    network: function(item) {
      const network = item.network;

      return network ? network.toLowerCase() : '';
    },

    nextAiring: function(item, direction) {
      const nextAiring = item.nextAiring;

      if (nextAiring) {
        return moment(nextAiring).unix();
      }

      if (direction === sortDirections.DESCENDING) {
        return 0;
      }

      return Number.MAX_VALUE;
    },

    previousAiring: function(item, direction) {
      const previousAiring = item.previousAiring;

      if (previousAiring) {
        return moment(previousAiring).unix();
      }

      if (direction === sortDirections.DESCENDING) {
        return -Number.MAX_VALUE;
      }

      return Number.MAX_VALUE;
    },

    episodeProgress: function(item) {
      const { statistics = {} } = item;

      const {
        episodeCount = 0,
        episodeFileCount
      } = statistics;

      const progress = episodeCount ? episodeFileCount / episodeCount * 100 : 100;

      return progress + episodeCount / 1000000;
    },

    episodeCount: function(item) {
      const { statistics = {} } = item;

      return statistics.totalEpisodeCount || 0;
    },

    seasonCount: function(item) {
      const { statistics = {} } = item;

      return statistics.seasonCount;
    },

    originalLanguage: function(item) {
      const { originalLanguage = {} } = item;

      return originalLanguage.name;
    },

    ratings: function(item) {
      const { ratings = {} } = item;

      return ratings.value;
    },

    monitorNewItems: function(item) {
      return item.monitorNewItems === 'all' ? 1 : 0;
    }
  },

  selectedFilterKey: 'all',

  filters,

  filterPredicates,

  filterBuilderProps
};

export const persistState = [
  'seriesIndex.sortKey',
  'seriesIndex.sortDirection',
  'seriesIndex.selectedFilterKey',
  'seriesIndex.customFilters',
  'seriesIndex.view',
  'seriesIndex.columns',
  'seriesIndex.posterOptions',
  'seriesIndex.overviewOptions',
  'seriesIndex.tableOptions'
];

//
// Actions Types

export const SET_SERIES_SORT = 'seriesIndex/setSeriesSort';
export const SET_SERIES_FILTER = 'seriesIndex/setSeriesFilter';
export const SET_SERIES_VIEW = 'seriesIndex/setSeriesView';
export const SET_SERIES_TABLE_OPTION = 'seriesIndex/setSeriesTableOption';
export const SET_SERIES_POSTER_OPTION = 'seriesIndex/setSeriesPosterOption';
export const SET_SERIES_OVERVIEW_OPTION = 'seriesIndex/setSeriesOverviewOption';

//
// Action Creators

export const setSeriesSort = createAction(SET_SERIES_SORT);
export const setSeriesFilter = createAction(SET_SERIES_FILTER);
export const setSeriesView = createAction(SET_SERIES_VIEW);
export const setSeriesTableOption = createAction(SET_SERIES_TABLE_OPTION);
export const setSeriesPosterOption = createAction(SET_SERIES_POSTER_OPTION);
export const setSeriesOverviewOption = createAction(SET_SERIES_OVERVIEW_OPTION);

//
// Reducers

export const reducers = createHandleActions({

  [SET_SERIES_SORT]: createSetClientSideCollectionSortReducer(section),
  [SET_SERIES_FILTER]: createSetClientSideCollectionFilterReducer(section),

  [SET_SERIES_VIEW]: function(state, { payload }) {
    return Object.assign({}, state, { view: payload.view });
  },

  [SET_SERIES_TABLE_OPTION]: createSetTableOptionReducer(section),

  [SET_SERIES_POSTER_OPTION]: function(state, { payload }) {
    const posterOptions = state.posterOptions;

    return {
      ...state,
      posterOptions: {
        ...posterOptions,
        ...payload
      }
    };
  },

  [SET_SERIES_OVERVIEW_OPTION]: function(state, { payload }) {
    const overviewOptions = state.overviewOptions;

    return {
      ...state,
      overviewOptions: {
        ...overviewOptions,
        ...payload
      }
    };
  }

}, defaultState, section);
