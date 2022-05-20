import _ from 'lodash';
import { createAction } from 'redux-actions';
import { createThunk, handleThunks } from 'Store/thunks';
import requestAction from 'Utilities/requestAction';
import updateSectionState from 'Utilities/State/updateSectionState';
import { set } from './baseActions';
import createHandleActions from './Creators/createHandleActions';

//
// Variables

export const section = 'providerOptions';

const lastActions = {};
let lastActionId = 0;

//
// State

export const defaultState = {
  items: [],
  isFetching: false,
  isPopulated: false,
  error: false
};

//
// Actions Types

export const FETCH_OPTIONS = 'providers/fetchOptions';
export const CLEAR_OPTIONS = 'providers/clearOptions';

//
// Action Creators

export const fetchOptions = createThunk(FETCH_OPTIONS);
export const clearOptions = createAction(CLEAR_OPTIONS);

//
// Action Handlers

export const actionHandlers = handleThunks({

  [FETCH_OPTIONS]: function(getState, payload, dispatch) {
    const subsection = `${section}.${payload.section}`;

    if (lastActions[payload.section] && _.isEqual(payload, lastActions[payload.section].payload)) {
      return;
    }

    const actionId = ++lastActionId;

    lastActions[payload.section] = {
      actionId,
      payload
    };

    dispatch(set({
      section: subsection,
      isFetching: true
    }));

    const promise = requestAction(payload);

    promise.done((data) => {
      if (lastActions[payload.section]) {
        if (lastActions[payload.section].actionId === actionId) {
          lastActions[payload.section] = null;
        }

        dispatch(set({
          section: subsection,
          isFetching: false,
          isPopulated: true,
          error: null,
          items: data.options || []
        }));
      }
    });

    promise.fail((xhr) => {
      if (lastActions[payload.section]) {
        if (lastActions[payload.section].actionId === actionId) {
          lastActions[payload.section] = null;
        }

        dispatch(set({
          section: subsection,
          isFetching: false,
          isPopulated: false,
          error: xhr
        }));
      }
    });
  }
});

//
// Reducers

export const reducers = createHandleActions({

  [CLEAR_OPTIONS]: function(state, { payload }) {
    const subsection = `${section}.${payload.section}`;

    lastActions[payload.section] = null;

    return updateSectionState(state, subsection, defaultState);
  }

}, {}, section);
