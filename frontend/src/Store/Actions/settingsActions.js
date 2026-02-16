import { handleThunks } from 'Store/thunks';
import createHandleActions from './Creators/createHandleActions';
import autoTaggings from './Settings/autoTaggings';
import autoTaggingSpecifications from './Settings/autoTaggingSpecifications';
import customFormats from './Settings/customFormats';
import customFormatSpecifications from './Settings/customFormatSpecifications';
import delayProfiles from './Settings/delayProfiles';
import downloadClientOptions from './Settings/downloadClientOptions';
import downloadClients from './Settings/downloadClients';
import importListExclusions from './Settings/importListExclusions';
import importListOptions from './Settings/importListOptions';
import importLists from './Settings/importLists';
import indexerFlags from './Settings/indexerFlags';
import indexerOptions from './Settings/indexerOptions';

export * from './Settings/autoTaggingSpecifications';
export * from './Settings/autoTaggings';
export * from './Settings/customFormatSpecifications.js';
export * from './Settings/customFormats';
export * from './Settings/delayProfiles';
export * from './Settings/downloadClients';
export * from './Settings/downloadClientOptions';
export * from './Settings/importListOptions';
export * from './Settings/importLists';
export * from './Settings/importListExclusions';
export * from './Settings/indexerFlags';
export * from './Settings/indexerOptions';

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
  importLists: importLists.defaultState,
  importListExclusions: importListExclusions.defaultState,
  importListOptions: importListOptions.defaultState,
  indexerFlags: indexerFlags.defaultState,
  indexerOptions: indexerOptions.defaultState
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
  ...importLists.actionHandlers,
  ...importListExclusions.actionHandlers,
  ...importListOptions.actionHandlers,
  ...indexerFlags.actionHandlers,
  ...indexerOptions.actionHandlers
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
  ...importLists.reducers,
  ...importListExclusions.reducers,
  ...importListOptions.reducers,
  ...indexerFlags.reducers,
  ...indexerOptions.reducers

}, defaultState, section);
