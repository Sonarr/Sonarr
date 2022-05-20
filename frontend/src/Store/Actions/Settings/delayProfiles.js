import _ from 'lodash';
import { createAction } from 'redux-actions';
import { update } from 'Store/Actions/baseActions';
import createFetchHandler from 'Store/Actions/Creators/createFetchHandler';
import createFetchSchemaHandler from 'Store/Actions/Creators/createFetchSchemaHandler';
import createRemoveItemHandler from 'Store/Actions/Creators/createRemoveItemHandler';
import createSaveProviderHandler from 'Store/Actions/Creators/createSaveProviderHandler';
import createSetSettingValueReducer from 'Store/Actions/Creators/Reducers/createSetSettingValueReducer';
import { createThunk } from 'Store/thunks';
import createAjaxRequest from 'Utilities/createAjaxRequest';

//
// Variables

const section = 'settings.delayProfiles';

//
// Actions Types

export const FETCH_DELAY_PROFILES = 'settings/delayProfiles/fetchDelayProfiles';
export const FETCH_DELAY_PROFILE_SCHEMA = 'settings/delayProfiles/fetchDelayProfileSchema';
export const SAVE_DELAY_PROFILE = 'settings/delayProfiles/saveDelayProfile';
export const DELETE_DELAY_PROFILE = 'settings/delayProfiles/deleteDelayProfile';
export const REORDER_DELAY_PROFILE = 'settings/delayProfiles/reorderDelayProfile';
export const SET_DELAY_PROFILE_VALUE = 'settings/delayProfiles/setDelayProfileValue';

//
// Action Creators

export const fetchDelayProfiles = createThunk(FETCH_DELAY_PROFILES);
export const fetchDelayProfileSchema = createThunk(FETCH_DELAY_PROFILE_SCHEMA);
export const saveDelayProfile = createThunk(SAVE_DELAY_PROFILE);
export const deleteDelayProfile = createThunk(DELETE_DELAY_PROFILE);
export const reorderDelayProfile = createThunk(REORDER_DELAY_PROFILE);

export const setDelayProfileValue = createAction(SET_DELAY_PROFILE_VALUE, (payload) => {
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
    [FETCH_DELAY_PROFILES]: createFetchHandler(section, '/delayprofile'),
    [FETCH_DELAY_PROFILE_SCHEMA]: createFetchSchemaHandler(section, '/delayprofile/schema'),

    [SAVE_DELAY_PROFILE]: createSaveProviderHandler(section, '/delayprofile'),
    [DELETE_DELAY_PROFILE]: createRemoveItemHandler(section, '/delayprofile'),

    [REORDER_DELAY_PROFILE]: (getState, payload, dispatch) => {
      const { id, moveIndex } = payload;
      const moveOrder = moveIndex + 1;
      const delayProfiles = getState().settings.delayProfiles.items;
      const moving = _.find(delayProfiles, { id });

      // Don't move if the order hasn't changed
      if (moving.order === moveOrder) {
        return;
      }

      const after = moveIndex > 0 ? _.find(delayProfiles, { order: moveIndex }) : null;
      const afterQueryParam = after ? `after=${after.id}` : '';

      const promise = createAjaxRequest({
        method: 'PUT',
        url: `/delayprofile/reorder/${id}?${afterQueryParam}`
      }).request;

      promise.done((data) => {
        dispatch(update({ section, data }));
      });
    }
  },

  //
  // Reducers

  reducers: {
    [SET_DELAY_PROFILE_VALUE]: createSetSettingValueReducer(section)
  }

};
