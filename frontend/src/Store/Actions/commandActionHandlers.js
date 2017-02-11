import $ from 'jquery';
import { batchActions } from 'redux-batched-actions';
import { isSameCommand } from 'Utilities/Command';
import { messageTypes } from 'Helpers/Props';
import * as types from './actionTypes';
import createFetchHandler from './Creators/createFetchHandler';
import { showMessage, hideMessage } from './appActions';
import { updateItem } from './baseActions';
import { addCommand, removeCommand } from './commandActions';

let lastCommand = null;
let lastCommandTimeout = null;
const removeCommandTimeoutIds = {};

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

const commandActionHandlers = {
  [types.FETCH_COMMANDS]: createFetchHandler('commands', '/command'),

  [types.EXECUTE_COMMAND](payload) {
    return (dispatch, getState) => {
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
    };
  },

  [types.UPDATE_COMMAND](payload) {
    return (dispatch, getState) => {
      dispatch(updateItem({ section: 'commands', ...payload }));

      showCommandMessage(payload, dispatch);
      scheduleRemoveCommand(payload, dispatch);
    };
  },

  [types.FINISH_COMMAND](payload) {
    return (dispatch, getState) => {
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
    };
  }

};

export default commandActionHandlers;
