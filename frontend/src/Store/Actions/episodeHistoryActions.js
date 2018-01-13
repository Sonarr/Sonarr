import $ from 'jquery';
import { createAction } from 'redux-actions';
import { batchActions } from 'redux-batched-actions';
import { createThunk, handleThunks } from 'Store/thunks';
import { sortDirections } from 'Helpers/Props';
import createHandleActions from './Creators/createHandleActions';
import { set, update } from './baseActions';

//
// Variables

export const section = 'episodeHistory';

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

export const FETCH_EPISODE_HISTORY = 'episodeHistory/fetchEpisodeHistory';
export const CLEAR_EPISODE_HISTORY = 'episodeHistory/clearEpisodeHistory';
export const EPISODE_HISTORY_MARK_AS_FAILED = 'episodeHistory/episodeHistoryMarkAsFailed';

//
// Action Creators

export const fetchEpisodeHistory = createThunk(FETCH_EPISODE_HISTORY);
export const clearEpisodeHistory = createAction(CLEAR_EPISODE_HISTORY);
export const episodeHistoryMarkAsFailed = createThunk(EPISODE_HISTORY_MARK_AS_FAILED);

//
// Action Handlers

export const actionHandlers = handleThunks({

  [FETCH_EPISODE_HISTORY]: function(getState, payload, dispatch) {
    dispatch(set({ section, isFetching: true }));

    const queryParams = {
      pageSize: 1000,
      page: 1,
      sortKey: 'date',
      sortDirection: sortDirections.DESCENDING,
      episodeId: payload.episodeId
    };

    const promise = $.ajax({
      url: '/history',
      data: queryParams
    });

    promise.done((data) => {
      dispatch(batchActions([
        update({ section, data: data.records }),

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

  [EPISODE_HISTORY_MARK_AS_FAILED]: function(getState, payload, dispatch) {
    const {
      historyId,
      episodeId
    } = payload;

    const promise = $.ajax({
      url: '/history/failed',
      method: 'POST',
      data: {
        id: historyId
      }
    });

    promise.done(() => {
      dispatch(fetchEpisodeHistory({ episodeId }));
    });
  }
});

//
// Reducers

export const reducers = createHandleActions({

  [CLEAR_EPISODE_HISTORY]: (state) => {
    return Object.assign({}, state, defaultState);
  }

}, defaultState, section);

