import { createAction } from 'redux-actions';
import { batchActions } from 'redux-batched-actions';
import createFetchSchemaHandler from 'Store/Actions/Creators/createFetchSchemaHandler';
import createClearReducer from 'Store/Actions/Creators/Reducers/createClearReducer';
import createSetProviderFieldValueReducer from 'Store/Actions/Creators/Reducers/createSetProviderFieldValueReducer';
import createSetSettingValueReducer from 'Store/Actions/Creators/Reducers/createSetSettingValueReducer';
import { createThunk } from 'Store/thunks';
import getNextId from 'Utilities/State/getNextId';
import getProviderState from 'Utilities/State/getProviderState';
import getSectionState from 'Utilities/State/getSectionState';
import selectProviderSchema from 'Utilities/State/selectProviderSchema';
import updateSectionState from 'Utilities/State/updateSectionState';
import translate from 'Utilities/String/translate';
import { removeItem, set, update, updateItem } from '../baseActions';

//
// Variables

const section = 'settings.autoTaggingSpecifications';

//
// Actions Types

export const FETCH_AUTO_TAGGING_SPECIFICATIONS = 'settings/autoTaggingSpecifications/fetchAutoTaggingSpecifications';
export const FETCH_AUTO_TAGGING_SPECIFICATION_SCHEMA = 'settings/autoTaggingSpecifications/fetchAutoTaggingSpecificationSchema';
export const SELECT_AUTO_TAGGING_SPECIFICATION_SCHEMA = 'settings/autoTaggingSpecifications/selectAutoTaggingSpecificationSchema';
export const SET_AUTO_TAGGING_SPECIFICATION_VALUE = 'settings/autoTaggingSpecifications/setAutoTaggingSpecificationValue';
export const SET_AUTO_TAGGING_SPECIFICATION_FIELD_VALUE = 'settings/autoTaggingSpecifications/setAutoTaggingSpecificationFieldValue';
export const SAVE_AUTO_TAGGING_SPECIFICATION = 'settings/autoTaggingSpecifications/saveAutoTaggingSpecification';
export const DELETE_AUTO_TAGGING_SPECIFICATION = 'settings/autoTaggingSpecifications/deleteAutoTaggingSpecification';
export const DELETE_ALL_AUTO_TAGGING_SPECIFICATION = 'settings/autoTaggingSpecifications/deleteAllAutoTaggingSpecification';
export const CLONE_AUTO_TAGGING_SPECIFICATION = 'settings/autoTaggingSpecifications/cloneAutoTaggingSpecification';
export const CLEAR_AUTO_TAGGING_SPECIFICATIONS = 'settings/autoTaggingSpecifications/clearAutoTaggingSpecifications';
export const CLEAR_AUTO_TAGGING_SPECIFICATION_PENDING = 'settings/autoTaggingSpecifications/clearAutoTaggingSpecificationPending';
//
// Action Creators

export const fetchAutoTaggingSpecifications = createThunk(FETCH_AUTO_TAGGING_SPECIFICATIONS);
export const fetchAutoTaggingSpecificationSchema = createThunk(FETCH_AUTO_TAGGING_SPECIFICATION_SCHEMA);
export const selectAutoTaggingSpecificationSchema = createAction(SELECT_AUTO_TAGGING_SPECIFICATION_SCHEMA);

export const saveAutoTaggingSpecification = createThunk(SAVE_AUTO_TAGGING_SPECIFICATION);
export const deleteAutoTaggingSpecification = createThunk(DELETE_AUTO_TAGGING_SPECIFICATION);
export const deleteAllAutoTaggingSpecification = createThunk(DELETE_ALL_AUTO_TAGGING_SPECIFICATION);

export const setAutoTaggingSpecificationValue = createAction(SET_AUTO_TAGGING_SPECIFICATION_VALUE, (payload) => {
  return {
    section,
    ...payload
  };
});

export const setAutoTaggingSpecificationFieldValue = createAction(SET_AUTO_TAGGING_SPECIFICATION_FIELD_VALUE, (payload) => {
  return {
    section,
    ...payload
  };
});

export const cloneAutoTaggingSpecification = createAction(CLONE_AUTO_TAGGING_SPECIFICATION);

export const clearAutoTaggingSpecification = createAction(CLEAR_AUTO_TAGGING_SPECIFICATIONS);

export const clearAutoTaggingSpecificationPending = createThunk(CLEAR_AUTO_TAGGING_SPECIFICATION_PENDING);

//
// Details

export default {

  //
  // State

  defaultState: {
    isPopulated: false,
    error: null,
    isSchemaFetching: false,
    isSchemaPopulated: false,
    schemaError: null,
    schema: [],
    selectedSchema: {},
    isSaving: false,
    saveError: null,
    items: [],
    pendingChanges: {}
  },

  //
  // Action Handlers

  actionHandlers: {
    [FETCH_AUTO_TAGGING_SPECIFICATION_SCHEMA]: createFetchSchemaHandler(section, '/autoTagging/schema'),

    [FETCH_AUTO_TAGGING_SPECIFICATIONS]: (getState, payload, dispatch) => {
      let tags = [];
      if (payload.id) {
        const cfState = getSectionState(getState(), 'settings.autoTaggings', true);
        const cf = cfState.items[cfState.itemMap[payload.id]];
        tags = cf.specifications.map((tag, i) => {
          return {
            id: i + 1,
            ...tag
          };
        });
      }

      dispatch(batchActions([
        update({ section, data: tags }),
        set({
          section,
          isPopulated: true
        })
      ]));
    },

    [SAVE_AUTO_TAGGING_SPECIFICATION]: (getState, payload, dispatch) => {
      const {
        id,
        ...otherPayload
      } = payload;

      const saveData = getProviderState({ id, ...otherPayload }, getState, section, false);

      // we have to set id since not actually posting to server yet
      if (!saveData.id) {
        saveData.id = getNextId(getState().settings.autoTaggingSpecifications.items);
      }

      dispatch(batchActions([
        updateItem({ section, ...saveData }),
        set({
          section,
          pendingChanges: {}
        })
      ]));
    },

    [DELETE_AUTO_TAGGING_SPECIFICATION]: (getState, payload, dispatch) => {
      const id = payload.id;
      return dispatch(removeItem({ section, id }));
    },

    [DELETE_ALL_AUTO_TAGGING_SPECIFICATION]: (getState, payload, dispatch) => {
      return dispatch(set({
        section,
        items: []
      }));
    },

    [CLEAR_AUTO_TAGGING_SPECIFICATION_PENDING]: (getState, payload, dispatch) => {
      return dispatch(set({
        section,
        pendingChanges: {}
      }));
    }
  },

  //
  // Reducers

  reducers: {
    [SET_AUTO_TAGGING_SPECIFICATION_VALUE]: createSetSettingValueReducer(section),
    [SET_AUTO_TAGGING_SPECIFICATION_FIELD_VALUE]: createSetProviderFieldValueReducer(section),

    [SELECT_AUTO_TAGGING_SPECIFICATION_SCHEMA]: (state, { payload }) => {
      return selectProviderSchema(state, section, payload, (selectedSchema) => {
        return selectedSchema;
      });
    },

    [CLONE_AUTO_TAGGING_SPECIFICATION]: function(state, { payload }) {
      const id = payload.id;
      const newState = getSectionState(state, section);
      const items = newState.items;
      const item = items.find((i) => i.id === id);
      const newId = getNextId(newState.items);
      const newItem = {
        ...item,
        id: newId,
        name: translate('DefaultNameCopiedSpecification', { name: item.name })
      };
      newState.items = [...items, newItem];
      newState.itemMap[newId] = newState.items.length - 1;

      return updateSectionState(state, section, newState);
    },

    [CLEAR_AUTO_TAGGING_SPECIFICATIONS]: createClearReducer(section, {
      isPopulated: false,
      error: null,
      items: []
    })
  }
};
