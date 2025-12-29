import { handleThunks } from 'Store/thunks';
import createHandleActions from './Creators/createHandleActions';
import autoTaggings from './Settings/autoTaggings';
import autoTaggingSpecifications from './Settings/autoTaggingSpecifications';
import customFormats from './Settings/customFormats';
import customFormatSpecifications from './Settings/customFormatSpecifications';
import delayProfiles from './Settings/delayProfiles';
import downloadClientOptions from './Settings/downloadClientOptions';
import downloadClients from './Settings/downloadClients';
import general from './Settings/general';
import importListExclusions from './Settings/importListExclusions';
import importListOptions from './Settings/importListOptions';
import importLists from './Settings/importLists';
import indexerFlags from './Settings/indexerFlags';
import indexerOptions from './Settings/indexerOptions';
import indexers from './Settings/indexers';
import languages from './Settings/languages';
import mediaManagement from './Settings/mediaManagement';
import metadata from './Settings/metadata';
import naming from './Settings/naming';
import namingExamples from './Settings/namingExamples';
import notifications from './Settings/notifications';
import qualityDefinitions from './Settings/qualityDefinitions';
import qualityProfiles from './Settings/qualityProfiles';

export * from './Settings/autoTaggingSpecifications';
export * from './Settings/autoTaggings';
export * from './Settings/customFormatSpecifications.js';
export * from './Settings/customFormats';
export * from './Settings/delayProfiles';
export * from './Settings/downloadClients';
export * from './Settings/downloadClientOptions';
export * from './Settings/general';
export * from './Settings/importListOptions';
export * from './Settings/importLists';
export * from './Settings/importListExclusions';
export * from './Settings/indexerFlags';
export * from './Settings/indexerOptions';
export * from './Settings/indexers';
export * from './Settings/languages';
export * from './Settings/mediaManagement';
export * from './Settings/metadata';
export * from './Settings/naming';
export * from './Settings/namingExamples';
export * from './Settings/notifications';
export * from './Settings/qualityDefinitions';
export * from './Settings/qualityProfiles';

//
// Variables

export const section = 'settings';

//
// State

export const defaultState = {
  advancedSettings: false,
  autoTaggingSpecifications: autoTaggingSpecifications.defaultState,
  autoTaggings: autoTaggings.defaultState,
  customFormatSpecifications: customFormatSpecifications.defaultState,
  customFormats: customFormats.defaultState,
  delayProfiles: delayProfiles.defaultState,
  downloadClients: downloadClients.defaultState,
  downloadClientOptions: downloadClientOptions.defaultState,
  general: general.defaultState,
  importLists: importLists.defaultState,
  importListExclusions: importListExclusions.defaultState,
  importListOptions: importListOptions.defaultState,
  indexerFlags: indexerFlags.defaultState,
  indexerOptions: indexerOptions.defaultState,
  indexers: indexers.defaultState,
  languages: languages.defaultState,
  mediaManagement: mediaManagement.defaultState,
  metadata: metadata.defaultState,
  naming: naming.defaultState,
  namingExamples: namingExamples.defaultState,
  notifications: notifications.defaultState,
  qualityDefinitions: qualityDefinitions.defaultState,
  qualityProfiles: qualityProfiles.defaultState
};

export const persistState = [
  'settings.importListExclusions.pageSize'
];

//
// Action Handlers

export const actionHandlers = handleThunks({
  ...autoTaggingSpecifications.actionHandlers,
  ...autoTaggings.actionHandlers,
  ...customFormatSpecifications.actionHandlers,
  ...customFormats.actionHandlers,
  ...delayProfiles.actionHandlers,
  ...downloadClients.actionHandlers,
  ...downloadClientOptions.actionHandlers,
  ...general.actionHandlers,
  ...importLists.actionHandlers,
  ...importListExclusions.actionHandlers,
  ...importListOptions.actionHandlers,
  ...indexerFlags.actionHandlers,
  ...indexerOptions.actionHandlers,
  ...indexers.actionHandlers,
  ...languages.actionHandlers,
  ...mediaManagement.actionHandlers,
  ...metadata.actionHandlers,
  ...naming.actionHandlers,
  ...namingExamples.actionHandlers,
  ...notifications.actionHandlers,
  ...qualityDefinitions.actionHandlers,
  ...qualityProfiles.actionHandlers
});

//
// Reducers

export const reducers = createHandleActions({
  ...autoTaggingSpecifications.reducers,
  ...autoTaggings.reducers,
  ...customFormatSpecifications.reducers,
  ...customFormats.reducers,
  ...delayProfiles.reducers,
  ...downloadClients.reducers,
  ...downloadClientOptions.reducers,
  ...general.reducers,
  ...importLists.reducers,
  ...importListExclusions.reducers,
  ...importListOptions.reducers,
  ...indexerFlags.reducers,
  ...indexerOptions.reducers,
  ...indexers.reducers,
  ...languages.reducers,
  ...mediaManagement.reducers,
  ...metadata.reducers,
  ...naming.reducers,
  ...namingExamples.reducers,
  ...notifications.reducers,
  ...qualityDefinitions.reducers,
  ...qualityProfiles.reducers

}, defaultState, section);
