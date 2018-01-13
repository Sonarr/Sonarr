import { createAction } from 'redux-actions';
import { createThunk } from 'Store/thunks';
import selectProviderSchema from 'Utilities/State/selectProviderSchema';
import createSetSettingValueReducer from 'Store/Actions/Creators/Reducers/createSetSettingValueReducer';
import createSetProviderFieldValueReducer from 'Store/Actions/Creators/Reducers/createSetProviderFieldValueReducer';
import createFetchHandler from 'Store/Actions/Creators/createFetchHandler';
import createFetchSchemaHandler from 'Store/Actions/Creators/createFetchSchemaHandler';
import createSaveProviderHandler, { createCancelSaveProviderHandler } from 'Store/Actions/Creators/createSaveProviderHandler';
import createTestProviderHandler, { createCancelTestProviderHandler } from 'Store/Actions/Creators/createTestProviderHandler';
import createRemoveItemHandler from 'Store/Actions/Creators/createRemoveItemHandler';

//
// Variables

const section = 'settings.notifications';

//
// Actions Types

export const FETCH_NOTIFICATIONS = 'settings/notifications/fetchNotifications';
export const FETCH_NOTIFICATION_SCHEMA = 'settings/notifications/fetchNotificationSchema';
export const SELECT_NOTIFICATION_SCHEMA = 'settings/notifications/selectNotificationSchema';
export const SET_NOTIFICATION_VALUE = 'settings/notifications/setNotificationValue';
export const SET_NOTIFICATION_FIELD_VALUE = 'settings/notifications/setNotificationFieldValue';
export const SAVE_NOTIFICATION = 'settings/notifications/saveNotification';
export const CANCEL_SAVE_NOTIFICATION = 'settings/notifications/cancelSaveNotification';
export const DELETE_NOTIFICATION = 'settings/notifications/deleteNotification';
export const TEST_NOTIFICATION = 'settings/notifications/testNotification';
export const CANCEL_TEST_NOTIFICATION = 'settings/notifications/cancelTestNotification';

//
// Action Creators

export const fetchNotifications = createThunk(FETCH_NOTIFICATIONS);
export const fetchNotificationSchema = createThunk(FETCH_NOTIFICATION_SCHEMA);
export const selectNotificationSchema = createAction(SELECT_NOTIFICATION_SCHEMA);

export const saveNotification = createThunk(SAVE_NOTIFICATION);
export const cancelSaveNotification = createThunk(CANCEL_SAVE_NOTIFICATION);
export const deleteNotification = createThunk(DELETE_NOTIFICATION);
export const testNotification = createThunk(TEST_NOTIFICATION);
export const cancelTestNotification = createThunk(CANCEL_TEST_NOTIFICATION);

export const setNotificationValue = createAction(SET_NOTIFICATION_VALUE, (payload) => {
  return {
    section,
    ...payload
  };
});

export const setNotificationFieldValue = createAction(SET_NOTIFICATION_FIELD_VALUE, (payload) => {
  return {
    section,
    ...payload
  };
});

//
// Details

export default {

  //
  // State

  defaultState: {
    isFetching: false,
    isPopulated: false,
    error: null,
    isFetchingSchema: false,
    isSchemaPopulated: false,
    schemaError: null,
    schema: [],
    selectedSchema: {},
    isSaving: false,
    saveError: null,
    isTesting: false,
    items: [],
    pendingChanges: {}
  },

  //
  // Action Handlers

  actionHandlers: {
    [FETCH_NOTIFICATIONS]: createFetchHandler(section, '/notification'),
    [FETCH_NOTIFICATION_SCHEMA]: createFetchSchemaHandler(section, '/notification/schema'),

    [SAVE_NOTIFICATION]: createSaveProviderHandler(section, '/notification'),
    [CANCEL_SAVE_NOTIFICATION]: createCancelSaveProviderHandler(section),
    [DELETE_NOTIFICATION]: createRemoveItemHandler(section, '/notification'),
    [TEST_NOTIFICATION]: createTestProviderHandler(section, '/notification'),
    [CANCEL_TEST_NOTIFICATION]: createCancelTestProviderHandler(section)
  },

  //
  // Reducers

  reducers: {
    [SET_NOTIFICATION_VALUE]: createSetSettingValueReducer(section),
    [SET_NOTIFICATION_FIELD_VALUE]: createSetProviderFieldValueReducer(section),

    [SELECT_NOTIFICATION_SCHEMA]: (state, { payload }) => {
      return selectProviderSchema(state, section, payload, (selectedSchema) => {
        selectedSchema.onGrab = selectedSchema.supportsOnGrab;
        selectedSchema.onDownload = selectedSchema.supportsOnDownload;
        selectedSchema.onUpgrade = selectedSchema.supportsOnUpgrade;
        selectedSchema.onRename = selectedSchema.supportsOnRename;

        return selectedSchema;
      });
    }
  }

};
