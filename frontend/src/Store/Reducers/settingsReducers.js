import _ from 'lodash';
import { handleActions } from 'redux-actions';
import getSectionState from 'Utilities/State/getSectionState';
import selectProviderSchema from 'Utilities/State/selectProviderSchema';
import updateSectionState from 'Utilities/State/updateSectionState';
import * as types from 'Store/Actions/actionTypes';
import createSetReducer from './Creators/createSetReducer';
import createSetSettingValueReducer from './Creators/createSetSettingValueReducer';
import createSetProviderFieldValueReducer from './Creators/createSetProviderFieldValueReducer';
import createClearPendingChangesReducer from './Creators/createClearPendingChangesReducer';
import createUpdateReducer from './Creators/createUpdateReducer';
import createUpdateItemReducer from './Creators/createUpdateItemReducer';
import createRemoveItemReducer from './Creators/createRemoveItemReducer';
import createReducers from './Creators/createReducers';

export const defaultState = {
  ui: {
    isFetching: false,
    isPopulated: false,
    error: null,
    pendingChanges: {},
    isSaving: false,
    saveError: null,
    item: {}
  },

  mediaManagement: {
    isFetching: false,
    isPopulated: false,
    error: null,
    pendingChanges: {},
    isSaving: false,
    saveError: null,
    item: {}
  },

  naming: {
    isFetching: false,
    isPopulated: false,
    error: null,
    pendingChanges: {},
    isSaving: false,
    saveError: null,
    item: {}
  },

  namingExamples: {
    isFetching: false,
    isPopulated: false,
    error: null,
    item: {}
  },

  qualityProfiles: {
    isFetching: false,
    isPopulated: false,
    error: null,
    isDeleting: false,
    deleteError: null,
    isFetchingSchema: false,
    schemaPopulated: false,
    schemaError: null,
    schema: {},
    isSaving: false,
    saveError: null,
    items: [],
    pendingChanges: {}
  },

  languageProfiles: {
    isFetching: false,
    isPopulated: false,
    error: null,
    isDeleting: false,
    deleteError: null,
    isFetchingSchema: false,
    schemaPopulated: false,
    schemaError: null,
    schema: {},
    isSaving: false,
    saveError: null,
    items: [],
    pendingChanges: {}
  },

  delayProfiles: {
    isFetching: false,
    isPopulated: false,
    error: null,
    items: [],
    isSaving: false,
    saveError: null,
    pendingChanges: {}
  },

  qualityDefinitions: {
    isFetching: false,
    isPopulated: false,
    error: null,
    items: [],
    isSaving: false,
    saveError: null,
    pendingChanges: {}
  },

  indexers: {
    isFetching: false,
    isPopulated: false,
    error: null,
    isFetchingSchema: false,
    schemaPopulated: false,
    schemaError: null,
    schema: [],
    selectedSchema: {},
    isSaving: false,
    saveError: null,
    isTesting: false,
    items: [],
    pendingChanges: {}
  },

  indexerOptions: {
    isFetching: false,
    isPopulated: false,
    error: null,
    pendingChanges: {},
    isSaving: false,
    saveError: null,
    item: {}
  },

  restrictions: {
    isFetching: false,
    isPopulated: false,
    error: null,
    isSaving: false,
    saveError: null,
    items: [],
    pendingChanges: {}
  },

  downloadClients: {
    isFetching: false,
    isPopulated: false,
    error: null,
    isFetchingSchema: false,
    schemaPopulated: false,
    schemaError: null,
    schema: [],
    selectedSchema: {},
    isSaving: false,
    saveError: null,
    isTesting: false,
    items: [],
    pendingChanges: {}
  },

  downloadClientOptions: {
    isFetching: false,
    isPopulated: false,
    error: null,
    pendingChanges: {},
    isSaving: false,
    saveError: null,
    item: {}
  },

  remotePathMappings: {
    isFetching: false,
    isPopulated: false,
    error: null,
    items: [],
    isSaving: false,
    saveError: null,
    pendingChanges: {}
  },

  notifications: {
    isFetching: false,
    isPopulated: false,
    error: null,
    isFetchingSchema: false,
    schemaPopulated: false,
    schemaError: null,
    schema: [],
    selectedSchema: {},
    isSaving: false,
    saveError: null,
    isTesting: false,
    items: [],
    pendingChanges: {}
  },

  metadata: {
    isFetching: false,
    isPopulated: false,
    error: null,
    isSaving: false,
    saveError: null,
    items: [],
    pendingChanges: {}
  },

  general: {
    isFetching: false,
    isPopulated: false,
    error: null,
    pendingChanges: {},
    isSaving: false,
    saveError: null,
    item: {}
  },

  advancedSettings: false
};

export const persistState = [
  'settings.advancedSettings'
];

const propertyNames = [
  'ui',
  'mediaManagement',
  'naming',
  'namingExamples',
  'qualityDefinitions',
  'indexerOptions',
  'downloadClientOptions',
  'general'
];

const providerPropertyNames = [
  'qualityProfiles',
  'languageProfiles',
  'delayProfiles',
  'indexers',
  'restrictions',
  'downloadClients',
  'remotePathMappings',
  'notifications',
  'metadata'
];

const settingsReducers = handleActions({

  [types.TOGGLE_ADVANCED_SETTINGS]: (state, { payload }) => {
    return Object.assign({}, state, { advancedSettings: !state.advancedSettings });
  },

  [types.SET]: createReducers([...propertyNames, ...providerPropertyNames], createSetReducer),
  [types.UPDATE]: createReducers([...propertyNames, ...providerPropertyNames], createUpdateReducer),
  [types.UPDATE_ITEM]: createReducers(providerPropertyNames, createUpdateItemReducer),
  [types.CLEAR_PENDING_CHANGES]: createReducers([...propertyNames, ...providerPropertyNames], createClearPendingChangesReducer),

  [types.REMOVE_ITEM]: createReducers(providerPropertyNames, createRemoveItemReducer),

  [types.SET_UI_SETTINGS_VALUE]: createSetSettingValueReducer('ui'),
  [types.SET_MEDIA_MANAGEMENT_SETTINGS_VALUE]: createSetSettingValueReducer('mediaManagement'),
  [types.SET_NAMING_SETTINGS_VALUE]: createSetSettingValueReducer('naming'),
  [types.SET_QUALITY_PROFILE_VALUE]: createSetSettingValueReducer('qualityProfiles'),
  [types.SET_LANGUAGE_PROFILE_VALUE]: createSetSettingValueReducer('languageProfiles'),
  [types.SET_DELAY_PROFILE_VALUE]: createSetSettingValueReducer('delayProfiles'),

  [types.SET_QUALITY_DEFINITION_VALUE]: function(state, { payload }) {
    const section = 'qualityDefinitions';
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
  },

  [types.SET_INDEXER_VALUE]: createSetSettingValueReducer('indexers'),
  [types.SET_INDEXER_FIELD_VALUE]: createSetProviderFieldValueReducer('indexers'),
  [types.SET_INDEXER_OPTIONS_VALUE]: createSetSettingValueReducer('indexerOptions'),
  [types.SET_RESTRICTION_VALUE]: createSetSettingValueReducer('restrictions'),

  [types.SELECT_INDEXER_SCHEMA]: function(state, { payload }) {
    return selectProviderSchema(state, 'indexers', payload, (selectedSchema) => {
      selectedSchema.enableRss = selectedSchema.supportsRss;
      selectedSchema.enableSearch = selectedSchema.supportsSearch;

      return selectedSchema;
    });
  },

  [types.SET_DOWNLOAD_CLIENT_VALUE]: createSetSettingValueReducer('downloadClients'),
  [types.SET_DOWNLOAD_CLIENT_FIELD_VALUE]: createSetProviderFieldValueReducer('downloadClients'),

  [types.SELECT_DOWNLOAD_CLIENT_SCHEMA]: function(state, { payload }) {
    return selectProviderSchema(state, 'downloadClients', payload, (selectedSchema) => {
      selectedSchema.enable = true;

      return selectedSchema;
    });
  },

  [types.SET_DOWNLOAD_CLIENT_OPTIONS_VALUE]: createSetSettingValueReducer('downloadClientOptions'),
  [types.SET_REMOTE_PATH_MAPPING_VALUE]: createSetSettingValueReducer('remotePathMappings'),

  [types.SET_NOTIFICATION_VALUE]: createSetSettingValueReducer('notifications'),
  [types.SET_NOTIFICATION_FIELD_VALUE]: createSetProviderFieldValueReducer('notifications'),

  [types.SELECT_NOTIFICATION_SCHEMA]: function(state, { payload }) {
    return selectProviderSchema(state, 'notifications', payload, (selectedSchema) => {
      selectedSchema.onGrab = selectedSchema.supportsOnGrab;
      selectedSchema.onDownload = selectedSchema.supportsOnDownload;
      selectedSchema.onUpgrade = selectedSchema.supportsOnUpgrade;
      selectedSchema.onRename = selectedSchema.supportsOnRename;

      return selectedSchema;
    });
  },

  [types.SET_METADATA_VALUE]: createSetSettingValueReducer('metadata'),
  [types.SET_METADATA_FIELD_VALUE]: createSetProviderFieldValueReducer('metadata'),

  [types.SET_GENERAL_SETTINGS_VALUE]: createSetSettingValueReducer('general')

}, defaultState);

export default settingsReducers;
