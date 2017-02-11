import $ from 'jquery';
import { batchActions } from 'redux-batched-actions';
import { set, update, updateItem } from '../baseActions';

function createFetchHandler(section, url) {
  return function(payload = {}) {
    return function(dispatch, getState) {
      dispatch(set({ section, isFetching: true }));

      const {
        id,
        ...otherPayload
      } = payload;

      const promise = $.ajax({
        url: id == null ? url : `${url}/${id}`,
        data: otherPayload,
        traditional: true
      });

      promise.done((data) => {
        dispatch(batchActions([
          id == null ? update({ section, data }) : updateItem({ section, ...data }),

          set({
            section,
            isFetching: false,
            isPopulated: true,
            error: null
          })
        ]));
      });

      promise.fail((xhr) => {
        dispatch(set({
          section,
          isFetching: false,
          isPopulated: false,
          error: xhr
        }));
      });
    };
  };
}

export default createFetchHandler;
