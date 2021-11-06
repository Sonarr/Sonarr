import _ from 'lodash';
import { createAction } from 'redux-actions';
import { batchActions } from 'redux-batched-actions';
import monitorOptions from 'Utilities/Series/monitorOptions';
import getSectionState from 'Utilities/State/getSectionState';
import updateSectionState from 'Utilities/State/updateSectionState';
import createAjaxRequest from 'Utilities/createAjaxRequest';
import getNewSeries from 'Utilities/Series/getNewSeries';
import * as seriesTypes from 'Utilities/Series/seriesTypes';
import { createThunk, handleThunks } from 'Store/thunks';
import createSetSettingValueReducer from './Creators/Reducers/createSetSettingValueReducer';
import createHandleActions from './Creators/createHandleActions';
import { set, update, updateItem } from './baseActions';

//
// Variables

export const section = 'addSeries';
let abortCurrentRequest = null;

//
// State

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
    monitor: monitorOptions[0].key,
    qualityProfileId: 0,
    languageProfileId: 0,
    seriesType: seriesTypes.STANDARD,
    seasonFolder: true,
    searchForMissingEpisodes: false,
    searchForCutoffUnmetEpisodes: false,
    tags: []
  }
};

export const persistState = [
  'addSeries.defaults'
];

//
// Actions Types

export const LOOKUP_SERIES = 'addSeries/lookupSeries';
export const ADD_SERIES = 'addSeries/addSeries';
export const SET_ADD_SERIES_VALUE = 'addSeries/setAddSeriesValue';
export const CLEAR_ADD_SERIES = 'addSeries/clearAddSeries';
export const SET_ADD_SERIES_DEFAULT = 'addSeries/setAddSeriesDefault';

//
// Action Creators

export const lookupSeries = createThunk(LOOKUP_SERIES);
export const addSeries = createThunk(ADD_SERIES);
export const clearAddSeries = createAction(CLEAR_ADD_SERIES);
export const setAddSeriesDefault = createAction(SET_ADD_SERIES_DEFAULT);

export const setAddSeriesValue = createAction(SET_ADD_SERIES_VALUE, (payload) => {
  return {
    section,
    ...payload
  };
});

//
// Action Handlers

export const actionHandlers = handleThunks({

  [LOOKUP_SERIES]: function(getState, payload, dispatch) {
    dispatch(set({ section, isFetching: true }));

    if (abortCurrentRequest) {
      abortCurrentRequest();
    }

    const { request, abortRequest } = createAjaxRequest({
      url: '/series/lookup',
      data: {
        term: payload.term
      }
    });

    abortCurrentRequest = abortRequest;

    request.done((data) => {
      dispatch(batchActions([
        update({ section, data }),

        set({
          section,
          isFetching: false,
          isPopulated: true,
          error: null
        })
      ]));
    });

    request.fail((xhr) => {
      dispatch(set({
        section,
        isFetching: false,
        isPopulated: false,
        error: xhr.aborted ? null : xhr
      }));
    });
  },

  [ADD_SERIES]: function(getState, payload, dispatch) {
    dispatch(set({ section, isAdding: true }));

    const tvdbId = payload.tvdbId;
    const items = getState().addSeries.items;
    const newSeries = getNewSeries(_.cloneDeep(_.find(items, { tvdbId })), payload);

    const promise = createAjaxRequest({
      url: '/series',
      method: 'POST',
      dataType: 'json',
      contentType: 'application/json',
      data: JSON.stringify(newSeries)
    }).request;

    promise.done((data) => {
      dispatch(batchActions([
        updateItem({ section: 'series', ...data }),

        set({
          section,
          isAdding: false,
          isAdded: true,
          addError: null
        })
      ]));
    });

    promise.fail((xhr) => {
      dispatch(set({
        section,
        isAdding: false,
        isAdded: false,
        addError: xhr
      }));
    });
  }
});

//
// Reducers

export const reducers = createHandleActions({

  [SET_ADD_SERIES_VALUE]: createSetSettingValueReducer(section),

  [SET_ADD_SERIES_DEFAULT]: function(state, { payload }) {
    const newState = getSectionState(state, section);

    newState.defaults = {
      ...newState.defaults,
      ...payload
    };

    return updateSectionState(state, section, newState);
  },

  [CLEAR_ADD_SERIES]: function(state) {
    const {
      defaults,
      ...otherDefaultState
    } = defaultState;

    return Object.assign({}, state, otherDefaultState);
  }

}, defaultState, section);
