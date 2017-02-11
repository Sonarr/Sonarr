import { createAction } from 'redux-actions';
import * as types from './actionTypes';
import commandActionHandlers from './commandActionHandlers';

export const fetchCommands = commandActionHandlers[types.FETCH_COMMANDS];
export const executeCommand = commandActionHandlers[types.EXECUTE_COMMAND];
export const updateCommand = commandActionHandlers[types.UPDATE_COMMAND];
export const finishCommand = commandActionHandlers[types.FINISH_COMMAND];

export const addCommand = createAction(types.ADD_COMMAND);
export const removeCommand = createAction(types.REMOVE_COMMAND);

export const registerFinishCommandHandler = createAction(types.REGISTER_FINISH_COMMAND_HANDLER);
export const unregisterFinishCommandHandler = createAction(types.UNREGISTER_FINISH_COMMAND_HANDLER);
