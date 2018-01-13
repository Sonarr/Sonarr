import { createAction } from 'redux-actions';
import { createThunk, handleThunks } from 'Store/thunks';
import createFetchHandler from './Creators/createFetchHandler';
import createHandleActions from './Creators/createHandleActions';

//
// Variables

export const section = 'organizePreview';

//
// State

export const defaultState = {
  isFetching: false,
  isPopulated: false,
  error: null,
  items: []
};

//
// Actions Types

export const FETCH_ORGANIZE_PREVIEW = 'organizePreview/fetchOrganizePreview';
export const CLEAR_ORGANIZE_PREVIEW = 'organizePreview/clearOrganizePreview';

//
// Action Creators

export const fetchOrganizePreview = createThunk(FETCH_ORGANIZE_PREVIEW);
export const clearOrganizePreview = createAction(CLEAR_ORGANIZE_PREVIEW);

//
// Action Handlers

export const actionHandlers = handleThunks({

  [FETCH_ORGANIZE_PREVIEW]: createFetchHandler('organizePreview', '/rename')

});

//
// Reducers

export const reducers = createHandleActions({

  [CLEAR_ORGANIZE_PREVIEW]: (state) => {
    return Object.assign({}, state, defaultState);
  }

}, defaultState, section);
