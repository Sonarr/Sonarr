import { createAction } from 'redux-actions';
import createFetchHandler from 'Store/Actions/Creators/createFetchHandler';
import createRemoveItemHandler from 'Store/Actions/Creators/createRemoveItemHandler';
import createSaveProviderHandler from 'Store/Actions/Creators/createSaveProviderHandler';
import createSetSettingValueReducer from 'Store/Actions/Creators/Reducers/createSetSettingValueReducer';
import { createThunk } from 'Store/thunks';

//
// Variables

const section = 'settings.notificationTemplates';

//
// Actions Types

export const FETCH_NOTIFICATION_TEMPLATES = 'settings/notificationTemplates/fetchNotificationTemplates';
export const SAVE_NOTIFICATION_TEMPLATE = 'settings/notificationTemplates/saveNotificationTemplate';
export const DELETE_NOTIFICATION_TEMPLATE = 'settings/notificationTemplates/deleteNotificationTemplate';
export const SET_NOTIFICATION_TEMPLATE_VALUE = 'settings/notificationTemplates/setNotificationTemplateValue';

//
// Action Creators

export const fetchNotificationTemplates = createThunk(FETCH_NOTIFICATION_TEMPLATES);
export const saveNotificationTemplate = createThunk(SAVE_NOTIFICATION_TEMPLATE);
export const deleteNotificationTemplate = createThunk(DELETE_NOTIFICATION_TEMPLATE);

export const setNotificationTemplateValue = createAction(SET_NOTIFICATION_TEMPLATE_VALUE, (payload) => {
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
    isSaving: false,
    saveError: null,
    items: [],
    pendingChanges: {}
  },

  //
  // Action Handlers

  actionHandlers: {
    [FETCH_NOTIFICATION_TEMPLATES]: createFetchHandler(section, '/notificationtemplate'),

    [SAVE_NOTIFICATION_TEMPLATE]: createSaveProviderHandler(section, '/notificationtemplate'),

    [DELETE_NOTIFICATION_TEMPLATE]: createRemoveItemHandler(section, '/notificationtemplate')
  },

  //
  // Reducers

  reducers: {
    [SET_NOTIFICATION_TEMPLATE_VALUE]: createSetSettingValueReducer(section)
  }

};
