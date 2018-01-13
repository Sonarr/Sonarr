import $ from 'jquery';
import { createAction } from 'redux-actions';
import { batchActions } from 'redux-batched-actions';
import { sortDirections } from 'Helpers/Props';
import { createThunk, handleThunks } from 'Store/thunks';
import createSetClientSideCollectionSortReducer from './Creators/Reducers/createSetClientSideCollectionSortReducer';
import createSetClientSideCollectionFilterReducer from './Creators/Reducers/createSetClientSideCollectionFilterReducer';
import createHandleActions from './Creators/createHandleActions';
import { set, updateItem } from './baseActions';

//
// Variables

export const section = 'seriesEditor';

//
// State

export const defaultState = {
  isSaving: false,
  saveError: null,
  isDeleting: false,
  deleteError: null,
  sortKey: 'sortTitle',
  sortDirection: sortDirections.ASCENDING,
  secondarySortKey: 'sortTitle',
  secondarySortDirection: sortDirections.ASCENDING,
  selectedFilterKey: 'all',
  // filters come from seriesActions
  customFilters: []
  // filterPredicates come from seriesActions
};

export const persistState = [
  'seriesEditor.sortKey',
  'seriesEditor.sortDirection',
  'seriesEditor.selectedFilterKey',
  'seriesEditor.customFilters'
];

//
// Actions Types

export const SET_SERIES_EDITOR_SORT = 'seriesEditor/setSeriesEditorSort';
export const SET_SERIES_EDITOR_FILTER = 'seriesEditor/setSeriesEditorFilter';
export const SAVE_SERIES_EDITOR = 'seriesEditor/saveSeriesEditor';
export const BULK_DELETE_SERIES = 'seriesEditor/bulkDeleteSeries';

//
// Action Creators

export const setSeriesEditorSort = createAction(SET_SERIES_EDITOR_SORT);
export const setSeriesEditorFilter = createAction(SET_SERIES_EDITOR_FILTER);
export const saveSeriesEditor = createThunk(SAVE_SERIES_EDITOR);
export const bulkDeleteSeries = createThunk(BULK_DELETE_SERIES);

//
// Action Handlers

export const actionHandlers = handleThunks({
  [SAVE_SERIES_EDITOR]: function(getState, payload, dispatch) {
    dispatch(set({
      section,
      isSaving: true
    }));

    const promise = $.ajax({
      url: '/series/editor',
      method: 'PUT',
      data: JSON.stringify(payload),
      dataType: 'json'
    });

    promise.done((data) => {
      dispatch(batchActions([
        ...data.map((series) => {
          return updateItem({
            id: series.id,
            section: 'series',
            ...series
          });
        }),

        set({
          section,
          isSaving: false,
          saveError: null
        })
      ]));
    });

    promise.fail((xhr) => {
      dispatch(set({
        section,
        isSaving: false,
        saveError: xhr
      }));
    });
  },

  [BULK_DELETE_SERIES]: function(getState, payload, dispatch) {
    dispatch(set({
      section,
      isDeleting: true
    }));

    const promise = $.ajax({
      url: '/series/editor',
      method: 'DELETE',
      data: JSON.stringify(payload),
      dataType: 'json'
    });

    promise.done(() => {
      // SignaR will take care of removing the series from the collection

      dispatch(set({
        section,
        isDeleting: false,
        deleteError: null
      }));
    });

    promise.fail((xhr) => {
      dispatch(set({
        section,
        isDeleting: false,
        deleteError: xhr
      }));
    });
  }
});

//
// Reducers

export const reducers = createHandleActions({

  [SET_SERIES_EDITOR_SORT]: createSetClientSideCollectionSortReducer(section),
  [SET_SERIES_EDITOR_FILTER]: createSetClientSideCollectionFilterReducer(section)

}, defaultState, section);
