import { Dispatch } from 'redux';
import { createAction } from 'redux-actions';
import { batchActions } from 'redux-batched-actions';
import AppState from 'App/State/AppState';
import { createThunk, handleThunks } from 'Store/thunks';
import createAjaxRequest from 'Utilities/createAjaxRequest';
import { set, update } from './baseActions';
import createHandleActions from './Creators/createHandleActions';
import createClearReducer from './Creators/Reducers/createClearReducer';

interface FetchPayload {
  title: string;
}

//
// Variables

export const section = 'parse';
let parseTimeout: number | null = null;
let abortCurrentRequest: (() => void) | null = null;

//
// State

export const defaultState = {
  isFetching: false,
  isPopulated: false,
  error: null,
  item: {},
};

//
// Actions Types

export const FETCH = 'parse/fetch';
export const CLEAR = 'parse/clear';

//
// Action Creators

export const fetch = createThunk(FETCH);
export const clear = createAction(CLEAR);

//
// Action Handlers

export const actionHandlers = handleThunks({
  [FETCH]: function (
    _getState: () => AppState,
    payload: FetchPayload,
    dispatch: Dispatch
  ) {
    if (parseTimeout) {
      clearTimeout(parseTimeout);
    }

    parseTimeout = window.setTimeout(async () => {
      dispatch(set({ section, isFetching: true }));

      if (abortCurrentRequest) {
        abortCurrentRequest();
      }

      const { request, abortRequest } = createAjaxRequest({
        url: '/parse',
        data: {
          title: payload.title,
        },
      });

      try {
        const data = await request;

        dispatch(
          batchActions([
            update({ section, data }),

            set({
              section,
              isFetching: false,
              isPopulated: true,
              error: null,
            }),
          ])
        );
      } catch (error) {
        dispatch(
          set({
            section,
            isAdding: false,
            isAdded: false,
            addError: error,
          })
        );
      }

      abortCurrentRequest = abortRequest;
    }, 300);
  },
});

//
// Reducers

export const reducers = createHandleActions(
  {
    [CLEAR]: createClearReducer(section, defaultState),
  },
  defaultState,
  section
);
