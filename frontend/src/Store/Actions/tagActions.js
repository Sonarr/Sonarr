import $ from 'jquery';
import { createThunk, handleThunks } from 'Store/thunks';
import createFetchHandler from './Creators/createFetchHandler';
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
  items: []
};

//
// Actions Types

export const FETCH_TAGS = 'tags/fetchTags';
export const ADD_TAG = 'tags/addTag';

//
// Action Creators

export const fetchTags = createThunk(FETCH_TAGS);
export const addTag = createThunk(ADD_TAG);

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
  }
});

//
// Reducers
export const reducers = createHandleActions({}, defaultState, section);
