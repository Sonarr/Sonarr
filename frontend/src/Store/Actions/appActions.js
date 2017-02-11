import { createAction } from 'redux-actions';
import * as types from './actionTypes';

export const saveDimensions = createAction(types.SAVE_DIMENSIONS);
export const setVersion = createAction(types.SET_VERSION);
export const setIsSidebarVisible = createAction(types.SET_IS_SIDEBAR_VISIBLE);

export const setAppValue = createAction(types.SET_APP_VALUE, (payload) => {
  return {
    section: 'app',
    ...payload
  };
});

export const showMessage = createAction(types.SHOW_MESSAGE, (payload) => {
  return {
    section: 'messages',
    ...payload
  };
});

export const hideMessage = createAction(types.HIDE_MESSAGE, (payload) => {
  return {
    section: 'messages',
    ...payload
  };
});
