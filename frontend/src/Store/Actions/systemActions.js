import $ from 'jquery';
import { createAction } from 'redux-actions';
import serverSideCollectionHandlers from 'Utilities/serverSideCollectionHandlers';
import { filterTypes, sortDirections } from 'Helpers/Props';
import { createThunk, handleThunks } from 'Store/thunks';
import { setAppValue } from 'Store/Actions/appActions';
import createSetTableOptionReducer from './Creators/Reducers/createSetTableOptionReducer';
import createFetchHandler from './Creators/createFetchHandler';
import createRemoveItemHandler from './Creators/createRemoveItemHandler';
import createHandleActions from './Creators/createHandleActions';
import createServerSideCollectionHandlers from './Creators/createServerSideCollectionHandlers';
import { set } from './baseActions';

//
// Variables

export const section = 'system';
const backupsSection = 'system.backups';

//
// State

export const defaultState = {
  status: {
    isFetching: false,
    isPopulated: false,
    error: null,
    item: {}
  },

  health: {
    isFetching: false,
    isPopulated: false,
    error: null,
    items: []
  },

  diskSpace: {
    isFetching: false,
    isPopulated: false,
    error: null,
    items: []
  },

  tasks: {
    isFetching: false,
    isPopulated: false,
    error: null,
    items: []
  },

  backups: {
    isFetching: false,
    isPopulated: false,
    error: null,
    isRestoring: false,
    restoreError: null,
    isDeleting: false,
    deleteError: null,
    items: []
  },

  updates: {
    isFetching: false,
    isPopulated: false,
    error: null,
    items: []
  },

  logs: {
    isFetching: false,
    isPopulated: false,
    pageSize: 50,
    sortKey: 'time',
    sortDirection: sortDirections.DESCENDING,
    error: null,
    items: [],

    columns: [
      {
        name: 'level',
        isSortable: true,
        isVisible: true
      },
      {
        name: 'logger',
        label: 'Component',
        isSortable: true,
        isVisible: true
      },
      {
        name: 'message',
        label: 'Message',
        isVisible: true
      },
      {
        name: 'time',
        label: 'Time',
        isSortable: true,
        isVisible: true
      },
      {
        name: 'actions',
        columnLabel: 'Actions',
        isSortable: true,
        isVisible: true,
        isModifiable: false
      }
    ],

    selectedFilterKey: 'all',

    filters: [
      {
        key: 'all',
        label: 'All',
        filters: []
      },
      {
        key: 'info',
        label: 'Info',
        filters: [
          {
            key: 'level',
            value: 'info',
            type: filterTypes.EQUAL
          }
        ]
      },
      {
        key: 'warn',
        label: 'Warn',
        filters: [
          {
            key: 'level',
            value: 'warn',
            type: filterTypes.EQUAL
          }
        ]
      },
      {
        key: 'error',
        label: 'Error',
        filters: [
          {
            key: 'level',
            value: 'error',
            type: filterTypes.EQUAL
          }
        ]
      }
    ]
  },

  logFiles: {
    isFetching: false,
    isPopulated: false,
    error: null,
    items: []
  },

  updateLogFiles: {
    isFetching: false,
    isPopulated: false,
    error: null,
    items: []
  }
};

export const persistState = [
  'system.logs.pageSize',
  'system.logs.sortKey',
  'system.logs.sortDirection',
  'system.logs.selectedFilterKey'
];

//
// Actions Types

export const FETCH_STATUS = 'system/status/fetchStatus';
export const FETCH_HEALTH = 'system/health/fetchHealth';
export const FETCH_DISK_SPACE = 'system/diskSpace/fetchDiskSPace';

export const FETCH_TASK = 'system/tasks/fetchTask';
export const FETCH_TASKS = 'system/tasks/fetchTasks';

export const FETCH_BACKUPS = 'system/backups/fetchBackups';
export const RESTORE_BACKUP = 'system/backups/restoreBackup';
export const CLEAR_RESTORE_BACKUP = 'system/backups/clearRestoreBackup';
export const DELETE_BACKUP = 'system/backups/deleteBackup';

export const FETCH_UPDATES = 'system/updates/fetchUpdates';

export const FETCH_LOGS = 'system/logs/fetchLogs';
export const GOTO_FIRST_LOGS_PAGE = 'system/logs/gotoLogsFirstPage';
export const GOTO_PREVIOUS_LOGS_PAGE = 'system/logs/gotoLogsPreviousPage';
export const GOTO_NEXT_LOGS_PAGE = 'system/logs/gotoLogsNextPage';
export const GOTO_LAST_LOGS_PAGE = 'system/logs/gotoLogsLastPage';
export const GOTO_LOGS_PAGE = 'system/logs/gotoLogsPage';
export const SET_LOGS_SORT = 'system/logs/setLogsSort';
export const SET_LOGS_FILTER = 'system/logs/setLogsFilter';
export const SET_LOGS_TABLE_OPTION = 'system/logs/ssetLogsTableOption';

export const FETCH_LOG_FILES = 'system/logFiles/fetchLogFiles';
export const FETCH_UPDATE_LOG_FILES = 'system/updateLogFiles/fetchUpdateLogFiles';

export const RESTART = 'system/restart';
export const SHUTDOWN = 'system/shutdown';

//
// Action Creators

export const fetchStatus = createThunk(FETCH_STATUS);
export const fetchHealth = createThunk(FETCH_HEALTH);
export const fetchDiskSpace = createThunk(FETCH_DISK_SPACE);

export const fetchTask = createThunk(FETCH_TASK);
export const fetchTasks = createThunk(FETCH_TASKS);

export const fetchBackups = createThunk(FETCH_BACKUPS);
export const restoreBackup = createThunk(RESTORE_BACKUP);
export const clearRestoreBackup = createAction(CLEAR_RESTORE_BACKUP);
export const deleteBackup = createThunk(DELETE_BACKUP);

export const fetchUpdates = createThunk(FETCH_UPDATES);

export const fetchLogs = createThunk(FETCH_LOGS);
export const gotoLogsFirstPage = createThunk(GOTO_FIRST_LOGS_PAGE);
export const gotoLogsPreviousPage = createThunk(GOTO_PREVIOUS_LOGS_PAGE);
export const gotoLogsNextPage = createThunk(GOTO_NEXT_LOGS_PAGE);
export const gotoLogsLastPage = createThunk(GOTO_LAST_LOGS_PAGE);
export const gotoLogsPage = createThunk(GOTO_LOGS_PAGE);
export const setLogsSort = createThunk(SET_LOGS_SORT);
export const setLogsFilter = createThunk(SET_LOGS_FILTER);
export const setLogsTableOption = createAction(SET_LOGS_TABLE_OPTION);

export const fetchLogFiles = createThunk(FETCH_LOG_FILES);
export const fetchUpdateLogFiles = createThunk(FETCH_UPDATE_LOG_FILES);

export const restart = createThunk(RESTART);
export const shutdown = createThunk(SHUTDOWN);

//
// Action Handlers

export const actionHandlers = handleThunks({
  [FETCH_STATUS]: createFetchHandler('system.status', '/system/status'),
  [FETCH_HEALTH]: createFetchHandler('system.health', '/health'),
  [FETCH_DISK_SPACE]: createFetchHandler('system.diskSpace', '/diskspace'),
  [FETCH_TASK]: createFetchHandler('system.tasks', '/system/task'),
  [FETCH_TASKS]: createFetchHandler('system.tasks', '/system/task'),

  [FETCH_BACKUPS]: createFetchHandler(backupsSection, '/system/backup'),

  [RESTORE_BACKUP]: function(getState, payload, dispatch) {
    const {
      id,
      file
    } = payload;

    dispatch(set({
      section: backupsSection,
      isRestoring: true
    }));

    let ajaxOptions = null;

    if (id) {
      ajaxOptions = {
        url: `/system/backup/restore/${id}`,
        method: 'POST',
        contentType: 'application/json',
        dataType: 'json',
        data: JSON.stringify({
          id
        })
      };
    } else if (file) {
      const formData = new FormData();
      formData.append('restore', file);

      ajaxOptions = {
        url: '/system/backup/restore/upload',
        method: 'POST',
        processData: false,
        contentType: false,
        data: formData
      };
    } else {
      dispatch(set({
        section: backupsSection,
        isRestoring: false,
        restoreError: 'Error restoring backup'
      }));
    }

    const promise = $.ajax(ajaxOptions);

    promise.done((data) => {
      dispatch(set({
        section: backupsSection,
        isRestoring: false,
        restoreError: null
      }));
    });

    promise.fail((xhr) => {
      dispatch(set({
        section: backupsSection,
        isRestoring: false,
        restoreError: xhr
      }));
    });
  },

  [DELETE_BACKUP]: createRemoveItemHandler(backupsSection, '/system/backup'),

  [FETCH_UPDATES]: createFetchHandler('system.updates', '/update'),
  [FETCH_LOG_FILES]: createFetchHandler('system.logFiles', '/log/file'),
  [FETCH_UPDATE_LOG_FILES]: createFetchHandler('system.updateLogFiles', '/log/file/update'),

  ...createServerSideCollectionHandlers(
    'system.logs',
    '/log',
    fetchLogs,
    {
      [serverSideCollectionHandlers.FETCH]: FETCH_LOGS,
      [serverSideCollectionHandlers.FIRST_PAGE]: GOTO_FIRST_LOGS_PAGE,
      [serverSideCollectionHandlers.PREVIOUS_PAGE]: GOTO_PREVIOUS_LOGS_PAGE,
      [serverSideCollectionHandlers.NEXT_PAGE]: GOTO_NEXT_LOGS_PAGE,
      [serverSideCollectionHandlers.LAST_PAGE]: GOTO_LAST_LOGS_PAGE,
      [serverSideCollectionHandlers.EXACT_PAGE]: GOTO_LOGS_PAGE,
      [serverSideCollectionHandlers.SORT]: SET_LOGS_SORT,
      [serverSideCollectionHandlers.FILTER]: SET_LOGS_FILTER
    }
  ),

  [RESTART]: function(getState, payload, dispatch) {
    const promise = $.ajax({
      url: '/system/restart',
      method: 'POST'
    });

    promise.done(() => {
      dispatch(setAppValue({ isRestarting: true }));
    });
  },

  [SHUTDOWN]: function() {
    $.ajax({
      url: '/system/shutdown',
      method: 'POST'
    });
  }
});

//
// Reducers

export const reducers = createHandleActions({

  [CLEAR_RESTORE_BACKUP]: function(state, { payload }) {
    return {
      ...state,
      backups: {
        ...state.backups,
        isRestoring: false,
        restoreError: null
      }
    };
  },

  [SET_LOGS_TABLE_OPTION]: createSetTableOptionReducer('logs')

}, defaultState, section);
