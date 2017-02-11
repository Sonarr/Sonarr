import $ from 'jquery';
import serverSideCollectionHandlers from 'Utilities/serverSideCollectionHandlers';
import createServerSideCollectionHandlers from './Creators/createServerSideCollectionHandlers';
import * as types from './actionTypes';
import { updateItem } from './baseActions';

const section = 'history';

const historyActionHandlers = {
  ...createServerSideCollectionHandlers('history', '/history', (state) => state.history, {
    [serverSideCollectionHandlers.FETCH]: types.FETCH_HISTORY,
    [serverSideCollectionHandlers.FIRST_PAGE]: types.GOTO_FIRST_HISTORY_PAGE,
    [serverSideCollectionHandlers.PREVIOUS_PAGE]: types.GOTO_PREVIOUS_HISTORY_PAGE,
    [serverSideCollectionHandlers.NEXT_PAGE]: types.GOTO_NEXT_HISTORY_PAGE,
    [serverSideCollectionHandlers.LAST_PAGE]: types.GOTO_LAST_HISTORY_PAGE,
    [serverSideCollectionHandlers.EXACT_PAGE]: types.GOTO_HISTORY_PAGE,
    [serverSideCollectionHandlers.SORT]: types.SET_HISTORY_SORT,
    [serverSideCollectionHandlers.FILTER]: types.SET_HISTORY_FILTER
  }),

  [types.MARK_AS_FAILED]: function(payload) {
    return function(dispatch, getState) {
      const id = payload.id;

      dispatch(updateItem({
        section,
        id,
        isMarkingAsFailed: true
      }));

      const promise = $.ajax({
        url: '/history/failed',
        method: 'POST',
        data: {
          id
        }
      });

      promise.done(() => {
        dispatch(updateItem({
          section,
          id,
          isMarkingAsFailed: false,
          markAsFailedError: null
        }));
      });

      promise.fail((xhr) => {
        dispatch(updateItem({
          section,
          id,
          isMarkingAsFailed: false,
          markAsFailedError: xhr
        }));
      });
    };
  }
};

export default historyActionHandlers;
