import moment from 'moment';
import { createAction } from 'redux-actions';
import { sortDirections } from 'Helpers/Props';
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
    showSearchAction: false
  },

  tableOptions: {
    showBanners: false,
    showSearchAction: false
  },

  columns: [
    {
      name: 'status',
      columnLabel: 'Status',
      isSortable: true,
      isVisible: true,
      isModifiable: false
    },
    {
      name: 'sortTitle',
      label: 'Series Title',
      isSortable: true,
      isVisible: true,
      isModifiable: false
    },
    {
      name: 'seriesType',
      label: 'Type',
      isSortable: true,
      isVisible: false
    },
    {
      name: 'network',
      label: 'Network',
      isSortable: true,
      isVisible: true
    },
    {
      name: 'qualityProfileId',
      label: 'Quality Profile',
      isSortable: true,
      isVisible: true
    },
    {
      name: 'nextAiring',
      label: 'Next Airing',
      isSortable: true,
      isVisible: true
    },
    {
      name: 'previousAiring',
      label: 'Previous Airing',
      isSortable: true,
      isVisible: false
    },
    {
      name: 'originalLanguage',
      label: 'Original Language',
      isSortable: true,
      isVisible: false
    },
    {
      name: 'added',
      label: 'Added',
      isSortable: true,
      isVisible: false
    },
    {
      name: 'seasonCount',
      label: 'Seasons',
      isSortable: true,
      isVisible: true
    },
    {
      name: 'episodeProgress',
      label: 'Episodes',
      isSortable: true,
      isVisible: true
    },
    {
      name: 'episodeCount',
      label: 'Episode Count',
      isSortable: true,
      isVisible: false
    },
    {
      name: 'latestSeason',
      label: 'Latest Season',
      isSortable: true,
      isVisible: false
    },
    {
      name: 'year',
      label: 'Year',
      isSortable: true,
      isVisible: false
    },
    {
      name: 'path',
      label: 'Path',
      isSortable: true,
      isVisible: false
    },
    {
      name: 'sizeOnDisk',
      label: 'Size on Disk',
      isSortable: true,
      isVisible: false
    },
    {
      name: 'genres',
      label: 'Genres',
      isSortable: false,
      isVisible: false
    },
    {
      name: 'ratings',
      label: 'Rating',
      isSortable: true,
      isVisible: false
    },
    {
      name: 'certification',
      label: 'Certification',
      isSortable: false,
      isVisible: false
    },
    {
      name: 'releaseGroups',
      label: 'Release Groups',
      isSortable: false,
      isVisible: false
    },
    {
      name: 'tags',
      label: 'Tags',
      isSortable: true,
      isVisible: false
    },
    {
      name: 'useSceneNumbering',
      label: 'Scene Numbering',
      isSortable: true,
      isVisible: false
    },
    {
      name: 'actions',
      columnLabel: 'Actions',
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
