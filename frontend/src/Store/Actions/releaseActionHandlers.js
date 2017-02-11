import $ from 'jquery';
import createFetchHandler from './Creators/createFetchHandler';
import * as types from './actionTypes';
import { updateRelease } from './releaseActions';

const section = 'releases';

const releaseActionHandlers = {
  [types.FETCH_RELEASES]: createFetchHandler(section, '/release'),

  [types.GRAB_RELEASE]: function(payload) {
    return function(dispatch, getState) {
      const guid = payload.guid;

      dispatch(updateRelease({ guid, isGrabbing: true }));

      const promise = $.ajax({
        url: '/release',
        method: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(payload)
      });

      promise.done((data) => {
        dispatch(updateRelease({
          guid,
          isGrabbing: false,
          isGrabbed: true,
          grabError: null
        }));
      });

      promise.fail((xhr) => {
        const grabError = xhr.responseJSON && xhr.responseJSON.message || 'Failed to add to download queue';

        dispatch(updateRelease({
          guid,
          isGrabbing: false,
          isGrabbed: false,
          grabError
        }));
      });
    };
  }
};

export default releaseActionHandlers;
