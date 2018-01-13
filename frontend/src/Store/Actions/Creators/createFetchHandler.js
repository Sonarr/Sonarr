import { batchActions } from 'redux-batched-actions';
import createAjaxRequest from 'Utilities/createAjaxRequest';
import { set, update, updateItem } from '../baseActions';

export default function createFetchHandler(section, url) {
  return function(getState, payload, dispatch) {
    dispatch(set({ section, isFetching: true }));

    const {
      id,
      ...otherPayload
    } = payload;

    const { request, abortRequest } = createAjaxRequest({
      url: id == null ? url : `${url}/${id}`,
      data: otherPayload,
      traditional: true
    });

    request.done((data) => {
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

    request.fail((xhr) => {
      dispatch(set({
        section,
        isFetching: false,
        isPopulated: false,
        error: xhr.aborted ? null : xhr
      }));
    });

    return abortRequest;
  };
}
