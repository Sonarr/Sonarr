import { createAction } from 'redux-actions';
import { set } from 'Store/Actions/baseActions';
import createFetchHandler from 'Store/Actions/Creators/createFetchHandler';
import createRemoveItemHandler from 'Store/Actions/Creators/createRemoveItemHandler';
import createSaveProviderHandler from 'Store/Actions/Creators/createSaveProviderHandler';
import createSetSettingValueReducer from 'Store/Actions/Creators/Reducers/createSetSettingValueReducer';
import { createThunk } from 'Store/thunks';
import getSectionState from 'Utilities/State/getSectionState';
import updateSectionState from 'Utilities/State/updateSectionState';
import translate from 'Utilities/String/translate';

//
// Variables

const section = 'settings.autoTaggings';

//
// Actions Types

export const FETCH_AUTO_TAGGINGS = 'settings/autoTaggings/fetchAutoTaggings';
export const SAVE_AUTO_TAGGING = 'settings/autoTaggings/saveAutoTagging';
export const DELETE_AUTO_TAGGING = 'settings/autoTaggings/deleteAutoTagging';
export const SET_AUTO_TAGGING_VALUE = 'settings/autoTaggings/setAutoTaggingValue';
export const CLONE_AUTO_TAGGING = 'settings/autoTaggings/cloneAutoTagging';

//
// Action Creators

export const fetchAutoTaggings = createThunk(FETCH_AUTO_TAGGINGS);
export const saveAutoTagging = createThunk(SAVE_AUTO_TAGGING);
export const deleteAutoTagging = createThunk(DELETE_AUTO_TAGGING);

export const setAutoTaggingValue = createAction(SET_AUTO_TAGGING_VALUE, (payload) => {
  return {
    section,
    ...payload
  };
});

export const cloneAutoTagging = createAction(CLONE_AUTO_TAGGING);

//
// Details

export default {

  //
  // State

  defaultState: {
    isSchemaFetching: false,
    isSchemaPopulated: false,
    isFetching: false,
    isPopulated: false,
    schema: {
      removeTagsAutomatically: false,
      tags: []
    },
    error: null,
    isDeleting: false,
    deleteError: null,
    isSaving: false,
    saveError: null,
    items: [],
    pendingChanges: {}
  },

  //
  // Action Handlers

  actionHandlers: {
    [FETCH_AUTO_TAGGINGS]: createFetchHandler(section, '/autoTagging'),

    [DELETE_AUTO_TAGGING]: createRemoveItemHandler(section, '/autoTagging'),

    [SAVE_AUTO_TAGGING]: (getState, payload, dispatch) => {
      // move the format tags in as a pending change
      const state = getState();
      const pendingChanges = state.settings.autoTaggings.pendingChanges;
      pendingChanges.specifications = state.settings.autoTaggingSpecifications.items;
      dispatch(set({
        section,
        pendingChanges
      }));

      createSaveProviderHandler(section, '/autoTagging')(getState, payload, dispatch);
    }
  },

  //
  // Reducers

  reducers: {
    [SET_AUTO_TAGGING_VALUE]: createSetSettingValueReducer(section),

    [CLONE_AUTO_TAGGING]: function(state, { payload }) {
      const id = payload.id;
      const newState = getSectionState(state, section);
      const item = newState.items.find((i) => i.id === id);
      const pendingChanges = { ...item, id: 0 };
      delete pendingChanges.id;

      pendingChanges.name = translate('DefaultNameCopiedProfile', { name: pendingChanges.name });
      newState.pendingChanges = pendingChanges;

      return updateSectionState(state, section, newState);
    }
  }

};
