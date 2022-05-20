import { createAction } from 'redux-actions';
import createFetchHandler from 'Store/Actions/Creators/createFetchHandler';
import createRemoveItemHandler from 'Store/Actions/Creators/createRemoveItemHandler';
import createSaveProviderHandler from 'Store/Actions/Creators/createSaveProviderHandler';
import createSetSettingValueReducer from 'Store/Actions/Creators/Reducers/createSetSettingValueReducer';
import { createThunk } from 'Store/thunks';

//
// Variables

const section = 'settings.remotePathMappings';

//
// Actions Types

export const FETCH_REMOTE_PATH_MAPPINGS = 'settings/remotePathMappings/fetchRemotePathMappings';
export const SAVE_REMOTE_PATH_MAPPING = 'settings/remotePathMappings/saveRemotePathMapping';
export const DELETE_REMOTE_PATH_MAPPING = 'settings/remotePathMappings/deleteRemotePathMapping';
export const SET_REMOTE_PATH_MAPPING_VALUE = 'settings/remotePathMappings/setRemotePathMappingValue';

//
// Action Creators

export const fetchRemotePathMappings = createThunk(FETCH_REMOTE_PATH_MAPPINGS);
export const saveRemotePathMapping = createThunk(SAVE_REMOTE_PATH_MAPPING);
export const deleteRemotePathMapping = createThunk(DELETE_REMOTE_PATH_MAPPING);

export const setRemotePathMappingValue = createAction(SET_REMOTE_PATH_MAPPING_VALUE, (payload) => {
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
    items: [],
    isSaving: false,
    saveError: null,
    pendingChanges: {}
  },

  //
  // Action Handlers

  actionHandlers: {
    [FETCH_REMOTE_PATH_MAPPINGS]: createFetchHandler(section, '/remotepathmapping'),
    [SAVE_REMOTE_PATH_MAPPING]: createSaveProviderHandler(section, '/remotepathmapping'),
    [DELETE_REMOTE_PATH_MAPPING]: createRemoveItemHandler(section, '/remotepathmapping')
  },

  //
  // Reducers

  reducers: {
    [SET_REMOTE_PATH_MAPPING_VALUE]: createSetSettingValueReducer(section)
  }

};
