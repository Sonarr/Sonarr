import { createAction } from 'redux-actions';
import { setAppValue } from 'Store/Actions/appActions';
import { createThunk, handleThunks } from 'Store/thunks';
import createAjaxRequest from 'Utilities/createAjaxRequest';
import { pingServer } from './appActions';
import { set } from './baseActions';
import createFetchHandler from './Creators/createFetchHandler';
import createHandleActions from './Creators/createHandleActions';
import createRemoveItemHandler from './Creators/createRemoveItemHandler';

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

    const promise = createAjaxRequest(ajaxOptions).request;

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

  [RESTART]: function(getState, payload, dispatch) {
    const promise = createAjaxRequest({
      url: '/system/restart',
      method: 'POST'
    }).request;

    promise.done(() => {
      dispatch(setAppValue({ isRestarting: true }));
      dispatch(pingServer());
    });
  },

  [SHUTDOWN]: function() {
    createAjaxRequest({
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
  }

}, defaultState, section);
