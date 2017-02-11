import { handleActions } from 'redux-actions';
import getSectionState from 'Utilities/State/getSectionState';
import updateSectionState from 'Utilities/State/updateSectionState';
import * as types from 'Store/Actions/actionTypes';
import createSetReducer from './Creators/createSetReducer';
import createSetSettingValueReducer from './Creators/createSetSettingValueReducer';
import createUpdateReducer from './Creators/createUpdateReducer';
import createUpdateItemReducer from './Creators/createUpdateItemReducer';
import createRemoveItemReducer from './Creators/createRemoveItemReducer';

export const defaultState = {
  isFetching: false,
  isPopulated: false,
  error: null,
  isAdding: false,
  isAdded: false,
  addError: null,
  items: [],

  defaults: {
    rootFolderPath: '',
    monitor: 'allEpisodes',
    qualityProfileId: 0,
    languageProfileId: 0,
    seriesType: 'standard',
    seasonFolder: true,
    tags: []
  }
};

export const persistState = [
  'addSeries.defaults'
];

const reducerSection = 'addSeries';

const addSeriesReducers = handleActions({

  [types.SET]: createSetReducer(reducerSection),
  [types.UPDATE]: createUpdateReducer(reducerSection),
  [types.UPDATE_ITEM]: createUpdateItemReducer(reducerSection),
  [types.REMOVE_ITEM]: createRemoveItemReducer(reducerSection),

  [types.SET_ADD_SERIES_VALUE]: createSetSettingValueReducer(reducerSection),

  [types.SET_ADD_SERIES_DEFAULT]: function(state, { payload }) {
    const newState = getSectionState(state, reducerSection);

    newState.defaults = {
      ...newState.defaults,
      ...payload
    };

    return updateSectionState(state, reducerSection, newState);
  },

  [types.CLEAR_ADD_SERIES]: function(state) {
    const {
      defaults,
      ...otherDefaultState
    } = defaultState;

    return Object.assign({}, state, otherDefaultState);
  }

}, defaultState);

export default addSeriesReducers;
