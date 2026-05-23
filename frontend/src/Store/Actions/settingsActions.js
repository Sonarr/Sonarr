import { handleThunks } from 'Store/thunks';
import createHandleActions from './Creators/createHandleActions';

//
// Variables

export const section = 'settings';

//
// State

export const defaultState = {
  advancedSettings: false
};

export const persistState = [
  'settings.importListExclusions.pageSize'
];

//
// Action Handlers

export const actionHandlers = handleThunks({});

//
// Reducers

export const reducers = createHandleActions({}, defaultState, section);
