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
import { removeItem, set, update, updateItem } from '../baseActions';

//
// Variables

const section = 'settings.customFormatSpecifications';

//
// Actions Types

export const FETCH_CUSTOM_FORMAT_SPECIFICATIONS = 'settings/customFormatSpecifications/fetchCustomFormatSpecifications';
export const FETCH_CUSTOM_FORMAT_SPECIFICATION_SCHEMA = 'settings/customFormatSpecifications/fetchCustomFormatSpecificationSchema';
export const SELECT_CUSTOM_FORMAT_SPECIFICATION_SCHEMA = 'settings/customFormatSpecifications/selectCustomFormatSpecificationSchema';
export const SET_CUSTOM_FORMAT_SPECIFICATION_VALUE = 'settings/customFormatSpecifications/setCustomFormatSpecificationValue';
export const SET_CUSTOM_FORMAT_SPECIFICATION_FIELD_VALUE = 'settings/customFormatSpecifications/setCustomFormatSpecificationFieldValue';
export const SAVE_CUSTOM_FORMAT_SPECIFICATION = 'settings/customFormatSpecifications/saveCustomFormatSpecification';
export const DELETE_CUSTOM_FORMAT_SPECIFICATION = 'settings/customFormatSpecifications/deleteCustomFormatSpecification';
export const DELETE_ALL_CUSTOM_FORMAT_SPECIFICATION = 'settings/customFormatSpecifications/deleteAllCustomFormatSpecification';
export const CLONE_CUSTOM_FORMAT_SPECIFICATION = 'settings/customFormatSpecifications/cloneCustomFormatSpecification';
export const CLEAR_CUSTOM_FORMAT_SPECIFICATIONS = 'settings/customFormatSpecifications/clearCustomFormatSpecifications';
export const CLEAR_CUSTOM_FORMAT_SPECIFICATION_PENDING = 'settings/customFormatSpecifications/clearCustomFormatSpecificationPending';
//
// Action Creators

export const fetchCustomFormatSpecifications = createThunk(FETCH_CUSTOM_FORMAT_SPECIFICATIONS);
export const fetchCustomFormatSpecificationSchema = createThunk(FETCH_CUSTOM_FORMAT_SPECIFICATION_SCHEMA);
export const selectCustomFormatSpecificationSchema = createAction(SELECT_CUSTOM_FORMAT_SPECIFICATION_SCHEMA);

export const saveCustomFormatSpecification = createThunk(SAVE_CUSTOM_FORMAT_SPECIFICATION);
export const deleteCustomFormatSpecification = createThunk(DELETE_CUSTOM_FORMAT_SPECIFICATION);
export const deleteAllCustomFormatSpecification = createThunk(DELETE_ALL_CUSTOM_FORMAT_SPECIFICATION);

export const setCustomFormatSpecificationValue = createAction(SET_CUSTOM_FORMAT_SPECIFICATION_VALUE, (payload) => {
  return {
    section,
    ...payload
  };
});

export const setCustomFormatSpecificationFieldValue = createAction(SET_CUSTOM_FORMAT_SPECIFICATION_FIELD_VALUE, (payload) => {
  return {
    section,
    ...payload
  };
});

export const cloneCustomFormatSpecification = createAction(CLONE_CUSTOM_FORMAT_SPECIFICATION);

export const clearCustomFormatSpecification = createAction(CLEAR_CUSTOM_FORMAT_SPECIFICATIONS);

export const clearCustomFormatSpecificationPending = createThunk(CLEAR_CUSTOM_FORMAT_SPECIFICATION_PENDING);

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
    [FETCH_CUSTOM_FORMAT_SPECIFICATION_SCHEMA]: createFetchSchemaHandler(section, '/customformat/schema'),

    [FETCH_CUSTOM_FORMAT_SPECIFICATIONS]: (getState, payload, dispatch) => {
      let tags = [];
      if (payload.id) {
        const cfState = getSectionState(getState(), 'settings.customFormats', true);
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

    [SAVE_CUSTOM_FORMAT_SPECIFICATION]: (getState, payload, dispatch) => {
      const {
        id,
        ...otherPayload
      } = payload;

      const saveData = getProviderState({ id, ...otherPayload }, getState, section, false);

      // we have to set id since not actually posting to server yet
      if (!saveData.id) {
        saveData.id = getNextId(getState().settings.customFormatSpecifications.items);
      }

      dispatch(batchActions([
        updateItem({ section, ...saveData }),
        set({
          section,
          pendingChanges: {}
        })
      ]));
    },

    [DELETE_CUSTOM_FORMAT_SPECIFICATION]: (getState, payload, dispatch) => {
      const id = payload.id;
      return dispatch(removeItem({ section, id }));
    },

    [DELETE_ALL_CUSTOM_FORMAT_SPECIFICATION]: (getState, payload, dispatch) => {
      return dispatch(set({
        section,
        items: []
      }));
    },

    [CLEAR_CUSTOM_FORMAT_SPECIFICATION_PENDING]: (getState, payload, dispatch) => {
      return dispatch(set({
        section,
        pendingChanges: {}
      }));
    }
  },

  //
  // Reducers

  reducers: {
    [SET_CUSTOM_FORMAT_SPECIFICATION_VALUE]: createSetSettingValueReducer(section),
    [SET_CUSTOM_FORMAT_SPECIFICATION_FIELD_VALUE]: createSetProviderFieldValueReducer(section),

    [SELECT_CUSTOM_FORMAT_SPECIFICATION_SCHEMA]: (state, { payload }) => {
      return selectProviderSchema(state, section, payload, (selectedSchema) => {
        return selectedSchema;
      });
    },

    [CLONE_CUSTOM_FORMAT_SPECIFICATION]: function(state, { payload }) {
      const id = payload.id;
      const newState = getSectionState(state, section);
      const items = newState.items;
      const item = items.find((i) => i.id === id);
      const newId = getNextId(newState.items);
      const newItem = {
        ...item,
        id: newId,
        name: `${item.name} - Copy`
      };
      newState.items = [...items, newItem];
      newState.itemMap[newId] = newState.items.length - 1;

      return updateSectionState(state, section, newState);
    },

    [CLEAR_CUSTOM_FORMAT_SPECIFICATIONS]: createClearReducer(section, {
      isPopulated: false,
      error: null,
      items: []
    })
  }
};
