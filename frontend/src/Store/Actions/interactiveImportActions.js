import moment from 'moment';
import { createAction } from 'redux-actions';
import { batchActions } from 'redux-batched-actions';
import { sortDirections } from 'Helpers/Props';
import { createThunk, handleThunks } from 'Store/thunks';
import createAjaxRequest from 'Utilities/createAjaxRequest';
import updateSectionState from 'Utilities/State/updateSectionState';
import naturalExpansion from 'Utilities/String/naturalExpansion';
import { set, update, updateItem } from './baseActions';
import createFetchHandler from './Creators/createFetchHandler';
import createHandleActions from './Creators/createHandleActions';
import createSetClientSideCollectionSortReducer from './Creators/Reducers/createSetClientSideCollectionSortReducer';

//
// Variables

export const section = 'interactiveImport';

const episodesSection = `${section}.episodes`;
let abortCurrentRequest = null;
let currentIds = [];

const MAXIMUM_RECENT_FOLDERS = 10;

//
// State

export const defaultState = {
  isFetching: false,
  isPopulated: false,
  error: null,
  items: [],
  originalItems: [],
  sortKey: 'quality',
  sortDirection: sortDirections.DESCENDING,
  recentFolders: [],
  importMode: 'chooseImportMode',
  sortPredicates: {
    relativePath: function(item, direction) {
      const relativePath = item.relativePath;

      return naturalExpansion(relativePath.toLowerCase());
    },

    series: function(item, direction) {
      const series = item.series;

      return series ? series.sortTitle : '';
    },

    quality: function(item, direction) {
      return item.qualityWeight || 0;
    }
  },

  episodes: {
    isFetching: false,
    isReprocessing: false,
    isPopulated: false,
    error: null,
    sortKey: 'episodeNumber',
    sortDirection: sortDirections.ASCENDING,
    items: []
  }
};

export const persistState = [
  'interactiveImport.sortKey',
  'interactiveImport.sortDirection',
  'interactiveImport.recentFolders',
  'interactiveImport.importMode'
];

//
// Actions Types

export const FETCH_INTERACTIVE_IMPORT_ITEMS = 'interactiveImport/fetchInteractiveImportItems';
export const REPROCESS_INTERACTIVE_IMPORT_ITEMS = 'interactiveImport/reprocessInteractiveImportItems';
export const SET_INTERACTIVE_IMPORT_SORT = 'interactiveImport/setInteractiveImportSort';
export const UPDATE_INTERACTIVE_IMPORT_ITEM = 'interactiveImport/updateInteractiveImportItem';
export const UPDATE_INTERACTIVE_IMPORT_ITEMS = 'interactiveImport/updateInteractiveImportItems';
export const CLEAR_INTERACTIVE_IMPORT = 'interactiveImport/clearInteractiveImport';
export const ADD_RECENT_FOLDER = 'interactiveImport/addRecentFolder';
export const REMOVE_RECENT_FOLDER = 'interactiveImport/removeRecentFolder';
export const SET_INTERACTIVE_IMPORT_MODE = 'interactiveImport/setInteractiveImportMode';

export const FETCH_INTERACTIVE_IMPORT_EPISODES = 'interactiveImport/fetchInteractiveImportEpisodes';
export const SET_INTERACTIVE_IMPORT_EPISODES_SORT = 'interactiveImport/setInteractiveImportEpisodesSort';
export const CLEAR_INTERACTIVE_IMPORT_EPISODES = 'interactiveImport/clearInteractiveImportEpisodes';

//
// Action Creators

export const fetchInteractiveImportItems = createThunk(FETCH_INTERACTIVE_IMPORT_ITEMS);
export const reprocessInteractiveImportItems = createThunk(REPROCESS_INTERACTIVE_IMPORT_ITEMS);
export const setInteractiveImportSort = createAction(SET_INTERACTIVE_IMPORT_SORT);
export const updateInteractiveImportItem = createAction(UPDATE_INTERACTIVE_IMPORT_ITEM);
export const updateInteractiveImportItems = createAction(UPDATE_INTERACTIVE_IMPORT_ITEMS);
export const clearInteractiveImport = createAction(CLEAR_INTERACTIVE_IMPORT);
export const addRecentFolder = createAction(ADD_RECENT_FOLDER);
export const removeRecentFolder = createAction(REMOVE_RECENT_FOLDER);
export const setInteractiveImportMode = createAction(SET_INTERACTIVE_IMPORT_MODE);

export const fetchInteractiveImportEpisodes = createThunk(FETCH_INTERACTIVE_IMPORT_EPISODES);
export const setInteractiveImportEpisodesSort = createAction(SET_INTERACTIVE_IMPORT_EPISODES_SORT);
export const clearInteractiveImportEpisodes = createAction(CLEAR_INTERACTIVE_IMPORT_EPISODES);

//
// Action Handlers
export const actionHandlers = handleThunks({
  [FETCH_INTERACTIVE_IMPORT_ITEMS]: function(getState, payload, dispatch) {
    if (!payload.downloadId && !payload.folder) {
      dispatch(set({ section, error: { message: '`downloadId` or `folder` is required.' } }));
      return;
    }

    dispatch(set({ section, isFetching: true }));

    const promise = createAjaxRequest({
      url: '/manualimport',
      data: payload
    }).request;

    promise.done((data) => {
      dispatch(batchActions([
        update({ section, data }),

        set({
          section,
          isFetching: false,
          isPopulated: true,
          error: null,
          originalItems: data
        })
      ]));
    });

    promise.fail((xhr) => {
      dispatch(set({
        section,
        isFetching: false,
        isPopulated: false,
        error: xhr
      }));
    });
  },

  [REPROCESS_INTERACTIVE_IMPORT_ITEMS]: function(getState, payload, dispatch) {
    if (abortCurrentRequest) {
      abortCurrentRequest();
    }

    dispatch(batchActions([
      ...currentIds.map((id) => updateItem({
        section,
        id,
        isReprocessing: false,
        updateOnly: true
      })),
      ...payload.ids.map((id) => updateItem({
        section,
        id,
        isReprocessing: true,
        updateOnly: true
      }))
    ]));

    const items = getState()[section].items;

    const requestPayload = payload.ids.map((id) => {
      const item = items.find((i) => i.id === id);

      return {
        id,
        path: item.path,
        seriesId: item.series ? item.series.id : undefined,
        seasonNumber: item.seasonNumber,
        episodeIds: (item.episodes || []).map((e) => e.id),
        quality: item.quality,
        languages: item.languages,
        releaseGroup: item.releaseGroup,
        downloadId: item.downloadId
      };
    });

    const { request, abortRequest } = createAjaxRequest({
      method: 'POST',
      url: '/manualimport',
      contentType: 'application/json',
      data: JSON.stringify(requestPayload)
    });

    abortCurrentRequest = abortRequest;
    currentIds = payload.ids;

    request.done((data) => {
      dispatch(batchActions(
        data.map((item) => updateItem({
          section,
          ...item,
          isReprocessing: false,
          updateOnly: true
        }))
      ));
    });

    request.fail((xhr) => {
      if (xhr.aborted) {
        return;
      }

      dispatch(batchActions(
        payload.ids.map((id) => updateItem({
          section,
          id,
          isReprocessing: false,
          updateOnly: true
        }))
      ));
    });
  },

  [FETCH_INTERACTIVE_IMPORT_EPISODES]: createFetchHandler('interactiveImport.episodes', '/episode')
});

//
// Reducers

export const reducers = createHandleActions({

  [UPDATE_INTERACTIVE_IMPORT_ITEM]: (state, { payload }) => {
    const id = payload.id;
    const newState = Object.assign({}, state);
    const items = newState.items;
    const index = items.findIndex((item) => item.id === id);
    const item = Object.assign({}, items[index], payload);

    newState.items = [...items];
    newState.items.splice(index, 1, item);

    return newState;
  },

  [UPDATE_INTERACTIVE_IMPORT_ITEMS]: (state, { payload }) => {
    const ids = payload.ids;
    const newState = Object.assign({}, state);
    const items = [...newState.items];

    ids.forEach((id) => {
      const index = items.findIndex((item) => item.id === id);
      const item = Object.assign({}, items[index], payload);

      items.splice(index, 1, item);
    });

    newState.items = items;

    return newState;
  },

  [ADD_RECENT_FOLDER]: function(state, { payload }) {
    const folder = payload.folder;
    const recentFolder = { folder, lastUsed: moment().toISOString() };
    const recentFolders = [...state.recentFolders];
    const index = recentFolders.findIndex((r) => r.folder === folder);

    if (index > -1) {
      recentFolders.splice(index, 1);
    }

    recentFolders.push(recentFolder);

    const sliceIndex = Math.max(recentFolders.length - MAXIMUM_RECENT_FOLDERS, 0);

    return Object.assign({}, state, { recentFolders: recentFolders.slice(sliceIndex) });
  },

  [REMOVE_RECENT_FOLDER]: function(state, { payload }) {
    const folder = payload.folder;
    const recentFolders = [...state.recentFolders];
    const index = recentFolders.findIndex((r) => r.folder === folder);

    recentFolders.splice(index, 1);

    return Object.assign({}, state, { recentFolders });
  },

  [CLEAR_INTERACTIVE_IMPORT]: function(state) {
    const newState = {
      ...defaultState,
      recentFolders: state.recentFolders,
      importMode: state.importMode
    };

    return newState;
  },

  [SET_INTERACTIVE_IMPORT_SORT]: createSetClientSideCollectionSortReducer(section),

  [SET_INTERACTIVE_IMPORT_MODE]: function(state, { payload }) {
    return Object.assign({}, state, { importMode: payload.importMode });
  },

  [SET_INTERACTIVE_IMPORT_EPISODES_SORT]: createSetClientSideCollectionSortReducer(episodesSection),

  [CLEAR_INTERACTIVE_IMPORT_EPISODES]: (state) => {
    return updateSectionState(state, episodesSection, {
      ...defaultState.episodes
    });
  }

}, defaultState, section);
