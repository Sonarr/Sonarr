import { createAction } from 'redux-actions';
import { batchActions } from 'redux-batched-actions';
import { sortDirections } from 'Helpers/Props';
import { createThunk, handleThunks } from 'Store/thunks';
import createAjaxRequest from 'Utilities/createAjaxRequest';
import { set, updateItem } from './baseActions';
import createHandleActions from './Creators/createHandleActions';
import createSetClientSideCollectionFilterReducer from './Creators/Reducers/createSetClientSideCollectionFilterReducer';
import createSetClientSideCollectionSortReducer from './Creators/Reducers/createSetClientSideCollectionSortReducer';
import createSetTableOptionReducer from './Creators/Reducers/createSetTableOptionReducer';
import { filterBuilderProps, filterPredicates, filters, sortPredicates } from './seriesActions';

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
  sortPredicates,
  selectedFilterKey: 'all',
  filters,
  filterPredicates,

  columns: [
    {
      name: 'status',
      columnLabel: 'Status',
      isSortable: true,
      isVisible: true,
      isModifiable: false
    },
    {
      name: 'sortTitle',
      label: 'Series Title',
      isSortable: true,
      isVisible: true,
      isModifiable: false
    },
    {
      name: 'qualityProfileId',
      label: 'Quality Profile',
      isSortable: true,
      isVisible: true
    },
    {
      name: 'languageProfileId',
      label: 'Language Profile',
      isSortable: true,
      isVisible: true
    },
    {
      name: 'seriesType',
      label: 'Type',
      isSortable: true,
      isVisible: true
    },
    {
      name: 'seasonFolder',
      label: 'Season Folder',
      isSortable: true,
      isVisible: true
    },
    {
      name: 'path',
      label: 'Path',
      isSortable: true,
      isVisible: true
    },
    {
      name: 'sizeOnDisk',
      label: 'Size on Disk',
      isSortable: true,
      isVisible: false
    },
    {
      name: 'tags',
      label: 'Tags',
      isSortable: true,
      isVisible: true
    }
  ],

  filterBuilderProps
};

export const persistState = [
  'seriesEditor.sortKey',
  'seriesEditor.sortDirection',
  'seriesEditor.selectedFilterKey',
  'seriesEditor.customFilters',
  'seriesEditor.columns'
];

//
// Actions Types

export const SET_SERIES_EDITOR_SORT = 'seriesEditor/setSeriesEditorSort';
export const SET_SERIES_EDITOR_FILTER = 'seriesEditor/setSeriesEditorFilter';
export const SAVE_SERIES_EDITOR = 'seriesEditor/saveSeriesEditor';
export const BULK_DELETE_SERIES = 'seriesEditor/bulkDeleteSeries';
export const SET_SERIES_EDITOR_TABLE_OPTION = 'seriesIndex/setSeriesEditorTableOption';

//
// Action Creators

export const setSeriesEditorSort = createAction(SET_SERIES_EDITOR_SORT);
export const setSeriesEditorFilter = createAction(SET_SERIES_EDITOR_FILTER);
export const saveSeriesEditor = createThunk(SAVE_SERIES_EDITOR);
export const bulkDeleteSeries = createThunk(BULK_DELETE_SERIES);
export const setSeriesEditorTableOption = createAction(SET_SERIES_EDITOR_TABLE_OPTION);

//
// Action Handlers

export const actionHandlers = handleThunks({
  [SAVE_SERIES_EDITOR]: function(getState, payload, dispatch) {
    dispatch(set({
      section,
      isSaving: true
    }));

    const promise = createAjaxRequest({
      url: '/series/editor',
      method: 'PUT',
      data: JSON.stringify(payload),
      dataType: 'json'
    }).request;

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

    const promise = createAjaxRequest({
      url: '/series/editor',
      method: 'DELETE',
      data: JSON.stringify(payload),
      dataType: 'json'
    }).request;

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

  [SET_SERIES_EDITOR_TABLE_OPTION]: createSetTableOptionReducer(section),
  [SET_SERIES_EDITOR_SORT]: createSetClientSideCollectionSortReducer(section),
  [SET_SERIES_EDITOR_FILTER]: createSetClientSideCollectionFilterReducer(section)

}, defaultState, section);
