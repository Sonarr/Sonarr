import { createAction } from 'redux-actions';
import createAjaxRequest from 'Utilities/createAjaxRequest';
import { filterBuilderTypes, filterBuilderValueTypes, filterTypePredicates, sortDirections } from 'Helpers/Props';
import { createThunk, handleThunks } from 'Store/thunks';
import createSetClientSideCollectionSortReducer from './Creators/Reducers/createSetClientSideCollectionSortReducer';
import createSetClientSideCollectionFilterReducer from './Creators/Reducers/createSetClientSideCollectionFilterReducer';
import createHandleActions from './Creators/createHandleActions';
import { set } from './baseActions';
import { fetchSeries, filters, filterPredicates } from './seriesActions';
import sortByName from 'Utilities/Array/sortByName';

//
// Variables

export const section = 'seasonPass';

//
// State

export const defaultState = {
  isSaving: false,
  saveError: null,
  sortKey: 'sortTitle',
  sortDirection: sortDirections.ASCENDING,
  secondarySortKey: 'sortTitle',
  secondarySortDirection: sortDirections.ASCENDING,
  selectedFilterKey: 'all',
  filters,
  filterPredicates: {
    ...filterPredicates,

    episodeProgress: function(item, filterValue, type) {
      const { statistics = {} } = item;

      const {
        episodeCount = 0,
        episodeFileCount
      } = statistics;

      const progress = episodeCount ?
        episodeFileCount / episodeCount * 100 :
        100;

      const predicate = filterTypePredicates[type];

      return predicate(progress, filterValue);
    }
  },

  filterBuilderProps: [
    {
      name: 'monitored',
      label: 'Monitored',
      type: filterBuilderTypes.EXACT,
      valueType: filterBuilderValueTypes.BOOL
    },
    {
      name: 'status',
      label: 'Status',
      type: filterBuilderTypes.EXACT,
      valueType: filterBuilderValueTypes.SERIES_STATUS
    },
    {
      name: 'seriesType',
      label: 'Type',
      type: filterBuilderTypes.EXACT,
      valueType: filterBuilderValueTypes.SERIES_TYPES
    },
    {
      name: 'network',
      label: 'Network',
      type: filterBuilderTypes.STRING,
      optionsSelector: function(items) {
        const tagList = items.reduce((acc, series) => {
          if (series.network) {
            acc.push({
              id: series.network,
              name: series.network
            });
          }

          return acc;
        }, []);

        return tagList.sort(sortByName);
      }
    },
    {
      name: 'qualityProfileId',
      label: 'Quality Profile',
      type: filterBuilderTypes.EXACT,
      valueType: filterBuilderValueTypes.QUALITY_PROFILE
    },
    {
      name: 'languageProfileId',
      label: 'Language Profile',
      type: filterBuilderTypes.EXACT,
      valueType: filterBuilderValueTypes.LANGUAGE_PROFILE
    },
    {
      name: 'nextAiring',
      label: 'Next Airing',
      type: filterBuilderTypes.DATE,
      valueType: filterBuilderValueTypes.DATE
    },
    {
      name: 'previousAiring',
      label: 'Previous Airing',
      type: filterBuilderTypes.DATE,
      valueType: filterBuilderValueTypes.DATE
    },
    {
      name: 'added',
      label: 'Added',
      type: filterBuilderTypes.DATE,
      valueType: filterBuilderValueTypes.DATE
    },
    {
      name: 'seasonCount',
      label: 'Season Count',
      type: filterBuilderTypes.NUMBER
    },
    {
      name: 'episodeProgress',
      label: 'Episode Progress',
      type: filterBuilderTypes.NUMBER
    },
    {
      name: 'path',
      label: 'Path',
      type: filterBuilderTypes.STRING
    },
    {
      name: 'rootFolderPath',
      label: 'Root Folder Path',
      type: filterBuilderTypes.EXACT
    },
    {
      name: 'sizeOnDisk',
      label: 'Size on Disk',
      type: filterBuilderTypes.NUMBER,
      valueType: filterBuilderValueTypes.BYTES
    },
    {
      name: 'genres',
      label: 'Genres',
      type: filterBuilderTypes.ARRAY,
      optionsSelector: function(items) {
        const tagList = items.reduce((acc, series) => {
          series.genres.forEach((genre) => {
            acc.push({
              id: genre,
              name: genre
            });
          });

          return acc;
        }, []);

        return tagList.sort(sortByName);
      }
    },
    {
      name: 'ratings',
      label: 'Rating',
      type: filterBuilderTypes.NUMBER
    },
    {
      name: 'certification',
      label: 'Certification',
      type: filterBuilderTypes.EXACT
    },
    {
      name: 'tags',
      label: 'Tags',
      type: filterBuilderTypes.ARRAY,
      valueType: filterBuilderValueTypes.TAG
    },
    {
      name: 'useSceneNumbering',
      label: 'Scene Numbering',
      type: filterBuilderTypes.EXACT
    }
  ]
};

export const persistState = [
  'seasonPass.sortKey',
  'seasonPass.sortDirection',
  'seasonPass.selectedFilterKey',
  'seasonPass.customFilters'
];

//
// Actions Types

export const SET_SEASON_PASS_SORT = 'seasonPass/setSeasonPassSort';
export const SET_SEASON_PASS_FILTER = 'seasonPass/setSeasonPassFilter';
export const SAVE_SEASON_PASS = 'seasonPass/saveSeasonPass';

//
// Action Creators

export const setSeasonPassSort = createAction(SET_SEASON_PASS_SORT);
export const setSeasonPassFilter = createAction(SET_SEASON_PASS_FILTER);
export const saveSeasonPass = createThunk(SAVE_SEASON_PASS);

//
// Action Handlers

export const actionHandlers = handleThunks({

  [SAVE_SEASON_PASS]: function(getState, payload, dispatch) {
    const {
      seriesIds,
      monitored,
      monitor
    } = payload;

    const series = [];

    seriesIds.forEach((id) => {
      const seriesToUpdate = { id };

      if (payload.hasOwnProperty('monitored')) {
        seriesToUpdate.monitored = monitored;
      }

      series.push(seriesToUpdate);
    });

    dispatch(set({
      section,
      isSaving: true
    }));

    const promise = createAjaxRequest({
      url: '/seasonPass',
      method: 'POST',
      data: JSON.stringify({
        series,
        monitoringOptions: { monitor }
      }),
      dataType: 'json'
    }).request;

    promise.done((data) => {
      dispatch(fetchSeries());

      dispatch(set({
        section,
        isSaving: false,
        saveError: null
      }));
    });

    promise.fail((xhr) => {
      dispatch(set({
        section,
        isSaving: false,
        saveError: xhr
      }));
    });
  }

});

//
// Reducers

export const reducers = createHandleActions({

  [SET_SEASON_PASS_SORT]: createSetClientSideCollectionSortReducer(section),
  [SET_SEASON_PASS_FILTER]: createSetClientSideCollectionFilterReducer(section)

}, defaultState, section);

