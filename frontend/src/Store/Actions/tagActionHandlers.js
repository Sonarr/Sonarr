import $ from 'jquery';
import * as types from './actionTypes';
import { update } from './baseActions';
import createFetchHandler from './Creators/createFetchHandler';

const tagActionHandlers = {
  [types.FETCH_TAGS]: createFetchHandler('tags', '/tag'),

  [types.ADD_TAG]: function(payload) {
    return (dispatch, getState) => {
      const promise = $.ajax({
        url: '/tag',
        method: 'POST',
        data: JSON.stringify(payload.tag)
      });

      promise.done((data) => {
        const tags = getState().tags.items.slice();
        tags.push(data);

        dispatch(update({ section: 'tags', data: tags }));
        payload.onTagCreated(data);
      });
    };
  }
};

export default tagActionHandlers;
