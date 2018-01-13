import { createAction } from 'redux-actions';
import { createThunk } from 'Store/thunks';
import createSetSettingValueReducer from 'Store/Actions/Creators/Reducers/createSetSettingValueReducer';
import createFetchHandler from 'Store/Actions/Creators/createFetchHandler';
import createSaveProviderHandler from 'Store/Actions/Creators/createSaveProviderHandler';
import createRemoveItemHandler from 'Store/Actions/Creators/createRemoveItemHandler';

//
// Variables

const section = 'settings.restrictions';

//
// Actions Types

export const FETCH_RESTRICTIONS = 'settings/restrictions/fetchRestrictions';
export const SAVE_RESTRICTION = 'settings/restrictions/saveRestriction';
export const DELETE_RESTRICTION = 'settings/restrictions/deleteRestriction';
export const SET_RESTRICTION_VALUE = 'settings/restrictions/setRestrictionValue';

//
// Action Creators

export const fetchRestrictions = createThunk(FETCH_RESTRICTIONS);
export const saveRestriction = createThunk(SAVE_RESTRICTION);
export const deleteRestriction = createThunk(DELETE_RESTRICTION);

export const setRestrictionValue = createAction(SET_RESTRICTION_VALUE, (payload) => {
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
    [FETCH_RESTRICTIONS]: createFetchHandler(section, '/restriction'),

    [SAVE_RESTRICTION]: createSaveProviderHandler(section, '/restriction'),

    [DELETE_RESTRICTION]: createRemoveItemHandler(section, '/restriction')
  },

  //
  // Reducers

  reducers: {
    [SET_RESTRICTION_VALUE]: createSetSettingValueReducer(section)
  }

};
