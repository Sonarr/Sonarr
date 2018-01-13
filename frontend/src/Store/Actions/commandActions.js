import _ from 'lodash';
import $ from 'jquery';
import { createAction } from 'redux-actions';
import { batchActions } from 'redux-batched-actions';
import { isSameCommand } from 'Utilities/Command';
import { messageTypes } from 'Helpers/Props';
import { createThunk, handleThunks } from 'Store/thunks';
import createFetchHandler from './Creators/createFetchHandler';
import createHandleActions from './Creators/createHandleActions';
import { showMessage, hideMessage } from './appActions';
import { updateItem } from './baseActions';

//
// Variables

export const section = 'commands';

let lastCommand = null;
let lastCommandTimeout = null;
const removeCommandTimeoutIds = {};

//
// State

export const defaultState = {
  isFetching: false,
  isPopulated: false,
  error: null,
  items: [],
  handlers: {}
};

//
// Actions Types

export const FETCH_COMMANDS = 'commands/fetchCommands';
export const EXECUTE_COMMAND = 'commands/executeCommand';
export const ADD_COMMAND = 'commands/updateCommand';
export const UPDATE_COMMAND = 'commands/finishCommand';
export const FINISH_COMMAND = 'commands/addCommand';
export const REMOVE_COMMAND = 'commands/removeCommand';

//
// Action Creators

export const fetchCommands = createThunk(FETCH_COMMANDS);
export const executeCommand = createThunk(EXECUTE_COMMAND);
export const updateCommand = createThunk(UPDATE_COMMAND);
export const finishCommand = createThunk(FINISH_COMMAND);
export const addCommand = createAction(ADD_COMMAND);
export const removeCommand = createAction(REMOVE_COMMAND);

//
// Helpers

function showCommandMessage(payload, dispatch) {
  const {
    id,
    name,
    manual,
    message,
    body = {},
    state
  } = payload;

  const {
    sendUpdatesToClient,
    suppressMessages
  } = body;

  if (!message || !body || !sendUpdatesToClient || suppressMessages) {
    return;
  }

  let type = messageTypes.INFO;
  let hideAfter = 0;

  if (state === 'completed') {
    type = messageTypes.SUCCESS;
    hideAfter = 4;
  } else if (state === 'failed') {
    type = messageTypes.ERROR;
    hideAfter = manual ? 10 : 4;
  }

  dispatch(showMessage({
    id,
    name,
    message,
    type,
    hideAfter
  }));
}

function scheduleRemoveCommand(command, dispatch) {
  const {
    id,
    state
  } = command;

  if (state === 'queued') {
    return;
  }

  const timeoutId = removeCommandTimeoutIds[id];

  if (timeoutId) {
    clearTimeout(timeoutId);
  }

  removeCommandTimeoutIds[id] = setTimeout(() => {
    dispatch(batchActions([
      removeCommand({ section: 'commands', id }),
      hideMessage({ id })
    ]));

    delete removeCommandTimeoutIds[id];
  }, 30000);
}

//
// Action Handlers

export const actionHandlers = handleThunks({
  [FETCH_COMMANDS]: createFetchHandler('commands', '/command'),

  [EXECUTE_COMMAND]: function(getState, payload, dispatch) {
    // TODO: show a message for the user
    if (lastCommand && isSameCommand(lastCommand, payload)) {
      console.warn('Please wait at least 5 seconds before running this command again');
    }

    lastCommand = payload;

    // clear last command after 5 seconds.
    if (lastCommandTimeout) {
      clearTimeout(lastCommandTimeout);
    }

    lastCommandTimeout = setTimeout(() => {
      lastCommand = null;
    }, 5000);

    const promise = $.ajax({
      url: '/command',
      method: 'POST',
      data: JSON.stringify(payload)
    });

    promise.done((data) => {
      dispatch(addCommand(data));
    });
  },

  [UPDATE_COMMAND]: function(getState, payload, dispatch) {
    dispatch(updateItem({ section: 'commands', ...payload }));

    showCommandMessage(payload, dispatch);
    scheduleRemoveCommand(payload, dispatch);
  },

  [FINISH_COMMAND]: function(getState, payload, dispatch) {
    const state = getState();
    const handlers = state.commands.handlers;

    Object.keys(handlers).forEach((key) => {
      const handler = handlers[key];

      if (handler.name === payload.name) {
        dispatch(handler.handler(payload));
      }
    });

    dispatch(removeCommand({ section: 'commands', ...payload }));
    showCommandMessage(payload, dispatch);
  }

});

//
// Reducers

export const reducers = createHandleActions({

  [ADD_COMMAND]: (state, { payload }) => {
    const newState = Object.assign({}, state);
    newState.items = [...state.items, payload];

    return newState;
  },

  [REMOVE_COMMAND]: (state, { payload }) => {
    const newState = Object.assign({}, state);
    newState.items = [...state.items];

    const index = _.findIndex(newState.items, { id: payload.id });

    if (index > -1) {
      newState.items.splice(index, 1);
    }

    return newState;
  }

}, defaultState, section);
