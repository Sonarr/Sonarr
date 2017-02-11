import _ from 'lodash';
import $ from 'jquery';
import { batchActions } from 'redux-batched-actions';
import createAjaxRequest from 'Utilities/createAjaxRequest';
import getNewSeries from 'Utilities/Series/getNewSeries';
import * as types from './actionTypes';
import { set, update, updateItem } from './baseActions';

let abortCurrentRequest = null;
const section = 'addSeries';

const addSeriesActionHandlers = {
  [types.LOOKUP_SERIES]: function(payload) {
    return function(dispatch, getState) {
      dispatch(set({ section, isFetching: true }));

      if (abortCurrentRequest) {
        abortCurrentRequest();
      }

      const { request, abortRequest } = createAjaxRequest()({
        url: '/series/lookup',
        data: {
          term: payload.term
        }
      });

      abortCurrentRequest = abortRequest;

      request.done((data) => {
        dispatch(batchActions([
          update({ section, data }),

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
    };
  },

  [types.ADD_SERIES]: function(payload) {
    return function(dispatch, getState) {
      dispatch(set({ section, isAdding: true }));

      const tvdbId = payload.tvdbId;
      const items = getState().addSeries.items;
      const newSeries = getNewSeries(_.cloneDeep(_.find(items, { tvdbId })), payload);

      const promise = $.ajax({
        url: '/series',
        method: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(newSeries)
      });

      promise.done((data) => {
        dispatch(batchActions([
          updateItem({ section: 'series', ...data }),

          set({
            section,
            isAdding: false,
            isAdded: true,
            addError: null
          })
        ]));
      });

      promise.fail((xhr) => {
        dispatch(set({
          section,
          isAdding: false,
          isAdded: false,
          addError: xhr
        }));
      });
    };
  }
};

export default addSeriesActionHandlers;
