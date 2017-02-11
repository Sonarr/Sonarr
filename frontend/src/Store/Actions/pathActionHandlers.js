import $ from 'jquery';
import * as types from './actionTypes';
import { set } from './baseActions';
import { updatePaths } from './pathActions';

const section = 'paths';

const pathActionHandlers = {
  [types.FETCH_PATHS](payload) {
    return (dispatch, getState) => {
      dispatch(set({ section, isFetching: true }));

      const promise = $.ajax({
        url: '/filesystem',
        data: {
          path: payload.path
        }
      });

      promise.done((data) => {
        dispatch(updatePaths({ path: payload.path, ...data }));

        dispatch(set({
          section,
          isFetching: false,
          isPopulated: true,
          error: null
        }));
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
  }
};

export default pathActionHandlers;
