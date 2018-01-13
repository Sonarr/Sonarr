import { createAction } from 'redux-actions';
import requestAction from 'Utilities/requestAction';
import updateSectionState from 'Utilities/State/updateSectionState';
import { createThunk, handleThunks } from 'Store/thunks';
import createHandleActions from './Creators/createHandleActions';
import { set } from './baseActions';

//
// Variables

export const section = 'devices';

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

export const FETCH_DEVICES = 'devices/fetchDevices';
export const CLEAR_DEVICES = 'devices/clearDevices';

//
// Action Creators

export const fetchDevices = createThunk(FETCH_DEVICES);
export const clearDevices = createAction(CLEAR_DEVICES);

//
// Action Handlers

export const actionHandlers = handleThunks({

  [FETCH_DEVICES]: function(getState, payload, dispatch) {
    const actionPayload = {
      action: 'getDevices',
      ...payload
    };

    dispatch(set({
      section,
      isFetching: true
    }));

    const promise = requestAction(actionPayload);

    promise.done((data) => {
      dispatch(set({
        section,
        isFetching: false,
        isPopulated: true,
        error: null,
        items: data.devices || []
      }));
    });

    promise.fail((xhr) => {
      dispatch(set({
        section,
        isFetching: false,
        isPopulated: false,
        error: xhr
      }));
    });
  }
});

//
// Reducers

export const reducers = createHandleActions({

  [CLEAR_DEVICES]: function(state) {
    return updateSectionState(state, section, defaultState);
  }

}, defaultState, section);
