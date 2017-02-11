import _ from 'lodash';
import $ from 'jquery';
import { batchActions } from 'redux-batched-actions';
import { set, updateServerSideCollection } from '../baseActions';

function createFetchServerSideCollectionHandler(section, url, getFromState) {
  return function(payload = {}) {
    return function(dispatch, getState) {
      dispatch(set({ section, isFetching: true }));

      const state = getFromState(getState());
      const sectionState = state.hasOwnProperty(section) ? state[section] : state;
      const page = payload.page || sectionState.page || 1;

      const data = Object.assign({ page },
        _.pick(sectionState, [
          'pageSize',
          'sortDirection',
          'sortKey',
          'filterKey',
          'filterValue'
        ]));

      const promise = $.ajax({
        url,
        data
      });

      promise.done((response) => {
        dispatch(batchActions([
          updateServerSideCollection({ section, data: response }),

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

export default createFetchServerSideCollectionHandler;
