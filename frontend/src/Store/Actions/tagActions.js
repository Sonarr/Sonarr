import $ from 'jquery';
import { createThunk, handleThunks } from 'Store/thunks';
import createFetchHandler from './Creators/createFetchHandler';
import createRemoveItemHandler from './Creators/createRemoveItemHandler';
import createHandleActions from './Creators/createHandleActions';
import { update } from './baseActions';

//
// Variables

export const section = 'tags';

//
// State

export const defaultState = {
  isFetching: false,
  isPopulated: false,
  error: null,
  items: [],

  details: {
    isFetching: false,
    isPopulated: false,
    error: null,
    items: []
  }
};

//
// Actions Types

export const FETCH_TAGS = 'tags/fetchTags';
export const ADD_TAG = 'tags/addTag';
export const DELETE_TAG = 'tags/deleteTag';
export const FETCH_TAG_DETAILS = 'tags/fetchTagDetails';

//
// Action Creators

export const fetchTags = createThunk(FETCH_TAGS);
export const addTag = createThunk(ADD_TAG);
export const deleteTag = createThunk(DELETE_TAG);
export const fetchTagDetails = createThunk(FETCH_TAG_DETAILS);

//
// Action Handlers

export const actionHandlers = handleThunks({
  [FETCH_TAGS]: createFetchHandler(section, '/tag'),

  [ADD_TAG]: function(getState, payload, dispatch) {
    const promise = $.ajax({
      url: '/tag',
      method: 'POST',
      data: JSON.stringify(payload.tag)
    });

    promise.done((data) => {
      const tags = getState().tags.items.slice();
      tags.push(data);

      dispatch(update({ section, data: tags }));
      payload.onTagCreated(data);
    });
  },

  [DELETE_TAG]: createRemoveItemHandler(section, '/tag'),
  [FETCH_TAG_DETAILS]: createFetchHandler('tags.details', '/tag/detail')

});

//
// Reducers
export const reducers = createHandleActions({}, defaultState, section);
