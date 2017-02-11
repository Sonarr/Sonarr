import { createAction } from 'redux-actions';
import * as types from './actionTypes';
import systemActionHandlers from './systemActionHandlers';

export const fetchStatus = systemActionHandlers[types.FETCH_STATUS];
export const fetchHealth = systemActionHandlers[types.FETCH_HEALTH];
export const fetchDiskSpace = systemActionHandlers[types.FETCH_DISK_SPACE];

export const fetchTask = systemActionHandlers[types.FETCH_TASK];
export const fetchTasks = systemActionHandlers[types.FETCH_TASKS];
export const fetchBackups = systemActionHandlers[types.FETCH_BACKUPS];
export const fetchUpdates = systemActionHandlers[types.FETCH_UPDATES];

export const fetchLogs = systemActionHandlers[types.FETCH_LOGS];
export const gotoLogsFirstPage = systemActionHandlers[types.GOTO_FIRST_LOGS_PAGE];
export const gotoLogsPreviousPage = systemActionHandlers[types.GOTO_PREVIOUS_LOGS_PAGE];
export const gotoLogsNextPage = systemActionHandlers[types.GOTO_NEXT_LOGS_PAGE];
export const gotoLogsLastPage = systemActionHandlers[types.GOTO_LAST_LOGS_PAGE];
export const gotoLogsPage = systemActionHandlers[types.GOTO_LOGS_PAGE];
export const setLogsSort = systemActionHandlers[types.SET_LOGS_SORT];
export const setLogsFilter = systemActionHandlers[types.SET_LOGS_FILTER];
export const setLogsTableOption = createAction(types.SET_LOGS_TABLE_OPTION);

export const fetchLogFiles = systemActionHandlers[types.FETCH_LOG_FILES];
export const fetchUpdateLogFiles = systemActionHandlers[types.FETCH_UPDATE_LOG_FILES];

export const restart = systemActionHandlers[types.RESTART];
export const shutdown = systemActionHandlers[types.SHUTDOWN];
