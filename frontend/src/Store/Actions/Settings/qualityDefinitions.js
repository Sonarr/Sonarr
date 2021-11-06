import _ from 'lodash';
import { createAction } from 'redux-actions';
import { batchActions } from 'redux-batched-actions';
import createAjaxRequest from 'Utilities/createAjaxRequest';
import getSectionState from 'Utilities/State/getSectionState';
import updateSectionState from 'Utilities/State/updateSectionState';
import { createThunk } from 'Store/thunks';
import createFetchHandler from 'Store/Actions/Creators/createFetchHandler';
import createSaveHandler from 'Store/Actions/Creators/createSaveHandler';
import { clearPendingChanges, set, update } from 'Store/Actions/baseActions';

//
// Variables

const section = 'settings.qualityDefinitions';

//
// Actions Types

export const FETCH_QUALITY_DEFINITIONS = 'settings/qualityDefinitions/fetchQualityDefinitions';
export const SAVE_QUALITY_DEFINITIONS = 'settings/qualityDefinitions/saveQualityDefinitions';
export const SET_QUALITY_DEFINITION_VALUE = 'settings/qualityDefinitions/setQualityDefinitionValue';

//
// Action Creators

export const fetchQualityDefinitions = createThunk(FETCH_QUALITY_DEFINITIONS);
export const saveQualityDefinitions = createThunk(SAVE_QUALITY_DEFINITIONS);

export const setQualityDefinitionValue = createAction(SET_QUALITY_DEFINITION_VALUE);

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
    [FETCH_QUALITY_DEFINITIONS]: createFetchHandler(section, '/qualitydefinition'),
    [SAVE_QUALITY_DEFINITIONS]: createSaveHandler(section, '/qualitydefinition'),

    [SAVE_QUALITY_DEFINITIONS]: function(getState, payload, dispatch) {
      const qualityDefinitions = getState().settings.qualityDefinitions;

      const upatedDefinitions = Object.keys(qualityDefinitions.pendingChanges).map((key) => {
        const id = parseInt(key);
        const pendingChanges = qualityDefinitions.pendingChanges[id] || {};
        const item = _.find(qualityDefinitions.items, { id });

        return Object.assign({}, item, pendingChanges);
      });

      // If there is nothing to save don't bother isSaving
      if (!upatedDefinitions || !upatedDefinitions.length) {
        return;
      }

      dispatch(set({
        section,
        isSaving: true
      }));

      const promise = createAjaxRequest({
        method: 'PUT',
        url: '/qualityDefinition/update',
        data: JSON.stringify(upatedDefinitions),
        contentType: 'application/json',
        dataType: 'json'
      }).request;

      promise.done((data) => {
        dispatch(batchActions([
          set({
            section,
            isSaving: false,
            saveError: null
          }),

          update({ section, data }),
          clearPendingChanges({ section })
        ]));
      });

      promise.fail((xhr) => {
        dispatch(set({
          section,
          isSaving: false,
          saveError: xhr
        }));
      });
    }
  },

  //
  // Reducers

  reducers: {
    [SET_QUALITY_DEFINITION_VALUE]: function(state, { payload }) {
      const { id, name, value } = payload;
      const newState = getSectionState(state, section);
      newState.pendingChanges = _.cloneDeep(newState.pendingChanges);

      const pendingState = newState.pendingChanges[id] || {};
      const currentValue = _.find(newState.items, { id })[name];

      if (currentValue === value) {
        delete pendingState[name];
      } else {
        pendingState[name] = value;
      }

      if (_.isEmpty(pendingState)) {
        delete newState.pendingChanges[id];
      } else {
        newState.pendingChanges[id] = pendingState;
      }

      return updateSectionState(state, section, newState);
    }
  }

};
