import _ from 'lodash';
import moment from 'moment';
import { handleActions } from 'redux-actions';
import updateSectionState from 'Utilities/State/updateSectionState';
import * as types from 'Store/Actions/actionTypes';
import { sortDirections } from 'Helpers/Props';
import createSetReducer from './Creators/createSetReducer';
import createUpdateReducer from './Creators/createUpdateReducer';
import createReducers from './Creators/createReducers';
import createSetClientSideCollectionSortReducer from './Creators/createSetClientSideCollectionSortReducer';

export const defaultState = {
  isFetching: false,
  isPopulated: false,
  error: null,
  items: [],
  sortKey: 'quality',
  sortDirection: sortDirections.DESCENDING,
  recentFolders: [],
  importMode: 'move',
  sortPredicates: {
    series: function(item, direction) {
      const series = item.series;

      return series ? series.sortTitle : '';
    },

    quality: function(item, direction) {
      return item.quality.qualityWeight;
    }
  },

  interactiveImportEpisodes: {
    isFetching: false,
    isPopulated: false,
    error: null,
    sortKey: 'episodeNumber',
    sortDirection: sortDirections.DESCENDING,
    items: []
  }
};

export const persistState = [
  'interactiveImport.recentFolders',
  'interactiveImport.importMode'
];

const reducerSection = 'interactiveImport';
const episodesSection = 'interactiveImportEpisodes';

const interactiveImportReducers = handleActions({

  [types.SET]: createReducers([reducerSection, episodesSection], createSetReducer),
  [types.UPDATE]: createReducers([reducerSection, episodesSection], createUpdateReducer),

  [types.UPDATE_INTERACTIVE_IMPORT_ITEM]: (state, { payload }) => {
    const id = payload.id;
    const newState = Object.assign({}, state);
    const items = newState.items;
    const index = _.findIndex(items, { id });
    const item = Object.assign({}, items[index], payload);

    newState.items = [...items];
    newState.items.splice(index, 1, item);

    return newState;
  },

  [types.ADD_RECENT_FOLDER]: function(state, { payload }) {
    const folder = payload.folder;
    const recentFolder = { folder, lastUsed: moment().toISOString() };
    const recentFolders = [...state.recentFolders];
    const index = _.findIndex(recentFolders, { folder });

    if (index > -1) {
      recentFolders.splice(index, 1, recentFolder);
    } else {
      recentFolders.push(recentFolder);
    }

    return Object.assign({}, state, { recentFolders });
  },

  [types.REMOVE_RECENT_FOLDER]: function(state, { payload }) {
    const folder = payload.folder;
    const recentFolders = _.remove([...state.recentFolders], { folder });

    return Object.assign({}, state, { recentFolders });
  },

  [types.CLEAR_INTERACTIVE_IMPORT]: function(state) {
    const newState = {
      ...defaultState,
      recentFolders: state.recentFolders,
      importMode: state.importMode
    };

    return newState;
  },

  [types.SET_INTERACTIVE_IMPORT_SORT]: createSetClientSideCollectionSortReducer(reducerSection),

  [types.SET_INTERACTIVE_IMPORT_MODE]: function(state, { payload }) {
    return Object.assign({}, state, { importMode: payload.importMode });
  },

  [types.SET_INTERACTIVE_IMPORT_EPISODES_SORT]: createSetClientSideCollectionSortReducer(episodesSection),

  [types.CLEAR_INTERACTIVE_IMPORT_EPISODES]: (state) => {
    const section = episodesSection;

    return updateSectionState(state, section, {
      ...defaultState.interactiveImportEpisodes
    });
  }

}, defaultState);

export default interactiveImportReducers;
