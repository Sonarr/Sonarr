import { createAction } from 'redux-actions';

//
// Action Types

export const SET = 'base/set';

export const UPDATE = 'base/update';
export const UPDATE_ITEM = 'base/updateItem';
export const UPDATE_SERVER_SIDE_COLLECTION = 'base/updateServerSideCollection';

export const SET_SETTING_VALUE = 'base/setSettingValue';
export const CLEAR_PENDING_CHANGES = 'base/clearPendingChanges';

export const REMOVE_ITEM = 'base/removeItem';

//
// Action Creators

export const set = createAction(SET);

export const update = createAction(UPDATE);
export const updateItem = createAction(UPDATE_ITEM);
export const updateServerSideCollection = createAction(UPDATE_SERVER_SIDE_COLLECTION);

export const setSettingValue = createAction(SET_SETTING_VALUE);
export const clearPendingChanges = createAction(CLEAR_PENDING_CHANGES);

export const removeItem = createAction(REMOVE_ITEM);
