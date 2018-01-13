import $ from 'jquery';
import { createAction } from 'redux-actions';
import customFilterHandlers from 'Utilities/customFilterHandlers';
import { filterBuilderTypes, filterBuilderValueTypes, filterTypes, sortDirections } from 'Helpers/Props';
import { createThunk, handleThunks } from 'Store/thunks';
import createSetClientSideCollectionSortReducer from './Creators/Reducers/createSetClientSideCollectionSortReducer';
import createSetClientSideCollectionFilterReducer from './Creators/Reducers/createSetClientSideCollectionFilterReducer';
import createCustomFilterReducers from './Creators/Reducers/createCustomFilterReducers';
import createFetchHandler from './Creators/createFetchHandler';
import createHandleActions from './Creators/createHandleActions';

//
// Variables

export const section = 'releases';
let abortCurrentRequest = null;

//
// State

export const defaultState = {
  isFetching: false,
  isPopulated: false,
  error: null,
  items: [],
  sortKey: 'releaseWeight',
  sortDirection: sortDirections.ASCENDING,
  sortPredicates: {
    peers: function(item, direction) {
      const seeders = item.seeders || 0;
      const leechers = item.leechers || 0;

      return seeders * 1000000 + leechers;
    },

    rejections: function(item, direction) {
      const rejections = item.rejections;
      const releaseWeight = item.releaseWeight;

      if (rejections.length !== 0) {
        return releaseWeight + 1000000;
      }

      return releaseWeight;
    }
  },

  selectedFilterKey: 'all',

  filters: [
    {
      key: 'all',
      label: 'All',
      filters: []
    },
    {
      key: 'season-pack',
      label: 'Season Pack',
      filters: [
        {
          key: 'fullSeason',
          value: true,
          type: filterTypes.EQUAL
        }
      ]
    },
    {
      key: 'not-season-pack',
      label: 'Not Season Pack',
      filters: [
        {
          key: 'fullSeason',
          value: false,
          type: filterTypes.EQUAL
        }
      ]
    }
  ],

  filterPredicates: {
    quality: function(item, value, type) {
      const qualityId = item.quality.quality.id;

      if (type === filterTypes.EQUAL) {
        return qualityId === value;
      }

      if (type === filterTypes.NOT_EQUAL) {
        return qualityId !== value;
      }

      // Default to false
      return false;
    }
  },

  filterBuilderProps: [
    {
      name: 'title',
      label: 'Title',
      type: filterBuilderTypes.STRING
    },
    {
      name: 'age',
      label: 'Age',
      type: filterBuilderTypes.NUMBER
    },
    {
      name: 'protocol',
      label: 'Protocol',
      type: filterBuilderTypes.EXACT,
      valueType: filterBuilderValueTypes.PROTOCOL
    },
    {
      name: 'indexerId',
      label: 'Indexer',
      type: filterBuilderTypes.EXACT,
      valueType: filterBuilderValueTypes.INDEXER
    },
    {
      name: 'size',
      label: 'Size',
      type: filterBuilderTypes.NUMBER
    },
    {
      name: 'seeders',
      label: 'Seeders',
      type: filterBuilderTypes.NUMBER
    },
    {
      name: 'peers',
      label: 'Peers',
      type: filterBuilderTypes.NUMBER
    },
    {
      name: 'quality',
      label: 'Quality',
      type: filterBuilderTypes.EXACT,
      valueType: filterBuilderValueTypes.QUALITY
    },
    {
      name: 'rejections',
      label: 'Rejections',
      type: filterBuilderTypes.NUMBER
    }
  ],

  customFilters: []
};

export const persistState = [
  'releases.selectedFilterKey',
  'releases.customFilters'
];

//
// Actions Types

export const FETCH_RELEASES = 'releases/fetchReleases';
export const CANCEL_FETCH_RELEASES = 'releases/cancelFetchReleases';
export const SET_RELEASES_SORT = 'releases/setReleasesSort';
export const CLEAR_RELEASES = 'releases/clearReleases';
export const GRAB_RELEASE = 'releases/grabRelease';
export const UPDATE_RELEASE = 'releases/updateRelease';
export const SET_RELEASES_FILTER = 'releases/setReleasesFilter';
export const ADD_RELEASES_CUSTOM_FILTER = 'releases/addReleasesCustomFilter';
export const REMOVE_RELEASES_CUSTOM_FILTER = 'releases/removeReleasesCustomFilter';
export const SAVE_RELEASES_CUSTOM_FILTER = 'releases/saveReleasesCustomFilter';

//
// Action Creators

export const fetchReleases = createThunk(FETCH_RELEASES);
export const cancelFetchReleases = createThunk(CANCEL_FETCH_RELEASES);
export const setReleasesSort = createAction(SET_RELEASES_SORT);
export const clearReleases = createAction(CLEAR_RELEASES);
export const grabRelease = createThunk(GRAB_RELEASE);
export const updateRelease = createAction(UPDATE_RELEASE);
export const setReleasesFilter = createAction(SET_RELEASES_FILTER);
export const addReleasesCustomFilter = createAction(ADD_RELEASES_CUSTOM_FILTER);
export const removeReleasesCustomFilter = createAction(REMOVE_RELEASES_CUSTOM_FILTER);
export const saveReleasesCustomFilter = createAction(SAVE_RELEASES_CUSTOM_FILTER);

//
// Helpers

const fetchReleasesHelper = createFetchHandler(section, '/release');

//
// Action Handlers

export const actionHandlers = handleThunks({

  [FETCH_RELEASES]: function(getState, payload, dispatch) {
    const abortRequest = fetchReleasesHelper(getState, payload, dispatch);

    abortCurrentRequest = abortRequest;
  },

  [CANCEL_FETCH_RELEASES]: function(getState, payload, dispatch) {
    if (abortCurrentRequest) {
      abortCurrentRequest = abortCurrentRequest();
    }
  },

  [GRAB_RELEASE]: function(getState, payload, dispatch) {
    const guid = payload.guid;

    dispatch(updateRelease({ guid, isGrabbing: true }));

    const promise = $.ajax({
      url: '/release',
      method: 'POST',
      contentType: 'application/json',
      data: JSON.stringify(payload)
    });

    promise.done((data) => {
      dispatch(updateRelease({
        guid,
        isGrabbing: false,
        isGrabbed: true,
        grabError: null
      }));
    });

    promise.fail((xhr) => {
      const grabError = xhr.responseJSON && xhr.responseJSON.message || 'Failed to add to download queue';

      dispatch(updateRelease({
        guid,
        isGrabbing: false,
        isGrabbed: false,
        grabError
      }));
    });
  }
});

//
// Reducers

export const reducers = createHandleActions({

  [CLEAR_RELEASES]: (state) => {
    return Object.assign({}, state, defaultState);
  },

  [UPDATE_RELEASE]: (state, { payload }) => {
    const guid = payload.guid;
    const newState = Object.assign({}, state);
    const items = newState.items;

    // Return early if there aren't any items (the user closed the modal)
    if (!items.length) {
      return;
    }

    const index = items.findIndex((item) => item.guid === guid);
    const item = Object.assign({}, items[index], payload);

    newState.items = [...items];
    newState.items.splice(index, 1, item);

    return newState;
  },

  [SET_RELEASES_SORT]: createSetClientSideCollectionSortReducer(section),
  [SET_RELEASES_FILTER]: createSetClientSideCollectionFilterReducer(section),

  ...createCustomFilterReducers(section, {
    [customFilterHandlers.REMOVE]: REMOVE_RELEASES_CUSTOM_FILTER,
    [customFilterHandlers.SAVE]: SAVE_RELEASES_CUSTOM_FILTER
  })

}, defaultState, section);
