import { createAction } from 'redux-actions';
import { batchActions } from 'redux-batched-actions';
import { createThunk, handleThunks } from 'Store/thunks';
import createAjaxRequest from 'Utilities/createAjaxRequest';
import { set, update } from './baseActions';
import createHandleActions from './Creators/createHandleActions';

//
// Variables

export const section = 'seriesHistory';

//
// State

export const defaultState = {
  isFetching: false,
  isPopulated: false,
  error: null,
  items: []
};

//
// Actions Types

export const FETCH_SERIES_HISTORY = 'seriesHistory/fetchSeriesHistory';
export const CLEAR_SERIES_HISTORY = 'seriesHistory/clearSeriesHistory';
export const SERIES_HISTORY_MARK_AS_FAILED = 'seriesHistory/seriesHistoryMarkAsFailed';

//
// Action Creators

export const fetchSeriesHistory = createThunk(FETCH_SERIES_HISTORY);
export const clearSeriesHistory = createAction(CLEAR_SERIES_HISTORY);
export const seriesHistoryMarkAsFailed = createThunk(SERIES_HISTORY_MARK_AS_FAILED);

//
// Action Handlers

export const actionHandlers = handleThunks({

  [FETCH_SERIES_HISTORY]: function(getState, payload, dispatch) {
    dispatch(set({ section, isFetching: true }));

    const promise = createAjaxRequest({
      url: '/history/series',
      data: payload
    }).request;

    promise.done((data) => {
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

    promise.fail((xhr) => {
      dispatch(set({
        section,
        isFetching: false,
        isPopulated: false,
        error: xhr
      }));
    });
  },

  [SERIES_HISTORY_MARK_AS_FAILED]: function(getState, payload, dispatch) {
    const {
      historyId,
      seriesId,
      seasonNumber
    } = payload;

    const promise = createAjaxRequest({
      url: `/history/failed/${historyId}`,
      method: 'POST',
      dataType: 'json'
    }).request;

    promise.done(() => {
      dispatch(fetchSeriesHistory({ seriesId, seasonNumber }));
    });
  }
});

//
// Reducers

export const reducers = createHandleActions({

  [CLEAR_SERIES_HISTORY]: (state) => {
    return Object.assign({}, state, defaultState);
  }

}, defaultState, section);

