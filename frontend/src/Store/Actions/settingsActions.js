import { handleThunks } from 'Store/thunks';
import createHandleActions from './Creators/createHandleActions';
import delayProfiles from './Settings/delayProfiles';
import downloadClientOptions from './Settings/downloadClientOptions';
import downloadClients from './Settings/downloadClients';

export * from './Settings/delayProfiles';
export * from './Settings/downloadClients';
export * from './Settings/downloadClientOptions';

//
// Variables

export const section = 'settings';

//
// State

export const defaultState = {
  advancedSettings: false,
  delayProfiles: delayProfiles.defaultState,
  downloadClients: downloadClients.defaultState,
  downloadClientOptions: downloadClientOptions.defaultState
};

export const persistState = [
  'settings.importListExclusions.pageSize'
];

//
// Action Handlers

export const actionHandlers = handleThunks({
  ...delayProfiles.actionHandlers,
  ...downloadClients.actionHandlers,
  ...downloadClientOptions.actionHandlers
});

//
// Reducers

export const reducers = createHandleActions({
  ...delayProfiles.reducers,
  ...downloadClients.reducers,
  ...downloadClientOptions.reducers

}, defaultState, section);
