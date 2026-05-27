import { handleThunks } from 'Store/thunks';
import createHandleActions from './Creators/createHandleActions';
import downloadClientOptions from './Settings/downloadClientOptions';
import downloadClients from './Settings/downloadClients';

export * from './Settings/downloadClients';
export * from './Settings/downloadClientOptions';

//
// Variables

export const section = 'settings';

//
// State

export const defaultState = {
  advancedSettings: false,
  downloadClients: downloadClients.defaultState,
  downloadClientOptions: downloadClientOptions.defaultState
};

export const persistState = [
  'settings.importListExclusions.pageSize'
];

//
// Action Handlers

export const actionHandlers = handleThunks({
  ...downloadClients.actionHandlers,
  ...downloadClientOptions.actionHandlers
});

//
// Reducers

export const reducers = createHandleActions({
  ...downloadClients.reducers,
  ...downloadClientOptions.reducers

}, defaultState, section);
