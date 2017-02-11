import $ from 'jquery';
import serverSideCollectionHandlers from 'Utilities/serverSideCollectionHandlers';
import * as types from './actionTypes';
import createFetchHandler from './Creators/createFetchHandler';
import createServerSideCollectionHandlers from './Creators/createServerSideCollectionHandlers';

const systemActionHandlers = {
  [types.FETCH_STATUS]: createFetchHandler('status', '/system/status'),
  [types.FETCH_HEALTH]: createFetchHandler('health', '/health'),
  [types.FETCH_DISK_SPACE]: createFetchHandler('diskSpace', '/diskspace'),
  [types.FETCH_TASK]: createFetchHandler('tasks', '/system/task'),
  [types.FETCH_TASKS]: createFetchHandler('tasks', '/system/task'),
  [types.FETCH_BACKUPS]: createFetchHandler('backups', '/system/backup'),
  [types.FETCH_UPDATES]: createFetchHandler('updates', '/update'),
  [types.FETCH_LOG_FILES]: createFetchHandler('logFiles', '/log/file'),
  [types.FETCH_UPDATE_LOG_FILES]: createFetchHandler('updateLogFiles', '/log/file/update'),

  ...createServerSideCollectionHandlers('logs', '/log', (state) => state.system, {
    [serverSideCollectionHandlers.FETCH]: types.FETCH_LOGS,
    [serverSideCollectionHandlers.FIRST_PAGE]: types.GOTO_FIRST_LOGS_PAGE,
    [serverSideCollectionHandlers.PREVIOUS_PAGE]: types.GOTO_PREVIOUS_LOGS_PAGE,
    [serverSideCollectionHandlers.NEXT_PAGE]: types.GOTO_NEXT_LOGS_PAGE,
    [serverSideCollectionHandlers.LAST_PAGE]: types.GOTO_LAST_LOGS_PAGE,
    [serverSideCollectionHandlers.EXACT_PAGE]: types.GOTO_LOGS_PAGE,
    [serverSideCollectionHandlers.SORT]: types.SET_LOGS_SORT,
    [serverSideCollectionHandlers.FILTER]: types.SET_LOGS_FILTER
  }),

  [types.RESTART]: function() {
    return function() {
      $.ajax({
        url: '/system/restart',
        method: 'POST'
      });
    };
  },

  [types.SHUTDOWN]: function() {
    return function() {
      $.ajax({
        url: '/system/shutdown',
        method: 'POST'
      });
    };
  }
};

export default systemActionHandlers;
