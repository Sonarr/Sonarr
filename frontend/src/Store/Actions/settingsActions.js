import { createAction } from 'redux-actions';
import { handleThunks } from 'Store/thunks';
import createHandleActions from './Creators/createHandleActions';
import customFormats from './Settings/customFormats';
import customFormatSpecifications from './Settings/customFormatSpecifications';
import delayProfiles from './Settings/delayProfiles';
import downloadClientOptions from './Settings/downloadClientOptions';
import downloadClients from './Settings/downloadClients';
import general from './Settings/general';
import importListExclusions from './Settings/importListExclusions';
import importLists from './Settings/importLists';
import indexerOptions from './Settings/indexerOptions';
import indexers from './Settings/indexers';
import languageProfiles from './Settings/languageProfiles';
import mediaManagement from './Settings/mediaManagement';
import metadata from './Settings/metadata';
import naming from './Settings/naming';
import namingExamples from './Settings/namingExamples';
import notifications from './Settings/notifications';
import qualityDefinitions from './Settings/qualityDefinitions';
import qualityProfiles from './Settings/qualityProfiles';
import releaseProfiles from './Settings/releaseProfiles';
import remotePathMappings from './Settings/remotePathMappings';
import ui from './Settings/ui';

export * from './Settings/customFormatSpecifications.js';
export * from './Settings/customFormats';
export * from './Settings/delayProfiles';
export * from './Settings/downloadClients';
export * from './Settings/downloadClientOptions';
export * from './Settings/general';
export * from './Settings/importLists';
export * from './Settings/importListExclusions';
export * from './Settings/indexerOptions';
export * from './Settings/indexers';
export * from './Settings/languageProfiles';
export * from './Settings/mediaManagement';
export * from './Settings/metadata';
export * from './Settings/naming';
export * from './Settings/namingExamples';
export * from './Settings/notifications';
export * from './Settings/qualityDefinitions';
export * from './Settings/qualityProfiles';
export * from './Settings/releaseProfiles';
export * from './Settings/remotePathMappings';
export * from './Settings/ui';

//
// Variables

export const section = 'settings';

//
// State

export const defaultState = {
  advancedSettings: false,

  customFormatSpecifications: customFormatSpecifications.defaultState,
  customFormats: customFormats.defaultState,
  delayProfiles: delayProfiles.defaultState,
  downloadClients: downloadClients.defaultState,
  downloadClientOptions: downloadClientOptions.defaultState,
  general: general.defaultState,
  importLists: importLists.defaultState,
  importListExclusions: importListExclusions.defaultState,
  indexerOptions: indexerOptions.defaultState,
  indexers: indexers.defaultState,
  languageProfiles: languageProfiles.defaultState,
  mediaManagement: mediaManagement.defaultState,
  metadata: metadata.defaultState,
  naming: naming.defaultState,
  namingExamples: namingExamples.defaultState,
  notifications: notifications.defaultState,
  qualityDefinitions: qualityDefinitions.defaultState,
  qualityProfiles: qualityProfiles.defaultState,
  releaseProfiles: releaseProfiles.defaultState,
  remotePathMappings: remotePathMappings.defaultState,
  ui: ui.defaultState
};

export const persistState = [
  'settings.advancedSettings'
];

//
// Actions Types

export const TOGGLE_ADVANCED_SETTINGS = 'settings/toggleAdvancedSettings';

//
// Action Creators

export const toggleAdvancedSettings = createAction(TOGGLE_ADVANCED_SETTINGS);

//
// Action Handlers

export const actionHandlers = handleThunks({
  ...customFormatSpecifications.actionHandlers,
  ...customFormats.actionHandlers,
  ...delayProfiles.actionHandlers,
  ...downloadClients.actionHandlers,
  ...downloadClientOptions.actionHandlers,
  ...general.actionHandlers,
  ...importLists.actionHandlers,
  ...importListExclusions.actionHandlers,
  ...indexerOptions.actionHandlers,
  ...indexers.actionHandlers,
  ...languageProfiles.actionHandlers,
  ...mediaManagement.actionHandlers,
  ...metadata.actionHandlers,
  ...naming.actionHandlers,
  ...namingExamples.actionHandlers,
  ...notifications.actionHandlers,
  ...qualityDefinitions.actionHandlers,
  ...qualityProfiles.actionHandlers,
  ...releaseProfiles.actionHandlers,
  ...remotePathMappings.actionHandlers,
  ...ui.actionHandlers
});

//
// Reducers

export const reducers = createHandleActions({

  [TOGGLE_ADVANCED_SETTINGS]: (state, { payload }) => {
    return Object.assign({}, state, { advancedSettings: !state.advancedSettings });
  },

  ...customFormatSpecifications.reducers,
  ...customFormats.reducers,
  ...delayProfiles.reducers,
  ...downloadClients.reducers,
  ...downloadClientOptions.reducers,
  ...general.reducers,
  ...importLists.reducers,
  ...importListExclusions.reducers,
  ...indexerOptions.reducers,
  ...indexers.reducers,
  ...languageProfiles.reducers,
  ...mediaManagement.reducers,
  ...metadata.reducers,
  ...naming.reducers,
  ...namingExamples.reducers,
  ...notifications.reducers,
  ...qualityDefinitions.reducers,
  ...qualityProfiles.reducers,
  ...releaseProfiles.reducers,
  ...remotePathMappings.reducers,
  ...ui.reducers

}, defaultState, section);
