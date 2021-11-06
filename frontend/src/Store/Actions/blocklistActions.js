import { createAction } from 'redux-actions';
import { batchActions } from 'redux-batched-actions';
import createAjaxRequest from 'Utilities/createAjaxRequest';
import serverSideCollectionHandlers from 'Utilities/serverSideCollectionHandlers';
import { createThunk, handleThunks } from 'Store/thunks';
import { sortDirections } from 'Helpers/Props';
import createClearReducer from './Creators/Reducers/createClearReducer';
import createSetTableOptionReducer from './Creators/Reducers/createSetTableOptionReducer';
import createHandleActions from './Creators/createHandleActions';
import createRemoveItemHandler from './Creators/createRemoveItemHandler';
import createServerSideCollectionHandlers from './Creators/createServerSideCollectionHandlers';
import { set, updateItem } from './baseActions';

//
// Variables

export const section = 'blocklist';

//
// State

export const defaultState = {
  isFetching: false,
  isPopulated: false,
  pageSize: 20,
  sortKey: 'date',
  sortDirection: sortDirections.DESCENDING,
  error: null,
  items: [],
  isRemoving: false,

  columns: [
    {
      name: 'series.sortTitle',
      label: 'Series Title',
      isSortable: true,
      isVisible: true
    },
    {
      name: 'sourceTitle',
      label: 'Source Title',
      isSortable: true,
      isVisible: true
    },
    {
      name: 'language',
      label: 'Language',
      isVisible: false
    },
    {
      name: 'quality',
      label: 'Quality',
      isVisible: true
    },
    {
      name: 'date',
      label: 'Date',
      isSortable: true,
      isVisible: true
    },
    {
      name: 'indexer',
      label: 'Indexer',
      isSortable: true,
      isVisible: false
    },
    {
      name: 'actions',
      columnLabel: 'Actions',
      isVisible: true,
      isModifiable: false
    }
  ]
};

export const persistState = [
  'blocklist.pageSize',
  'blocklist.sortKey',
  'blocklist.sortDirection',
  'blocklist.columns'
];

//
// Action Types

export const FETCH_BLOCKLIST = 'blocklist/fetchBlocklist';
export const GOTO_FIRST_BLOCKLIST_PAGE = 'blocklist/gotoBlocklistFirstPage';
export const GOTO_PREVIOUS_BLOCKLIST_PAGE = 'blocklist/gotoBlocklistPreviousPage';
export const GOTO_NEXT_BLOCKLIST_PAGE = 'blocklist/gotoBlocklistNextPage';
export const GOTO_LAST_BLOCKLIST_PAGE = 'blocklist/gotoBlocklistLastPage';
export const GOTO_BLOCKLIST_PAGE = 'blocklist/gotoBlocklistPage';
export const SET_BLOCKLIST_SORT = 'blocklist/setBlocklistSort';
export const SET_BLOCKLIST_TABLE_OPTION = 'blocklist/setBlocklistTableOption';
export const REMOVE_BLOCKLIST_ITEM = 'blocklist/removeBlocklistItem';
export const REMOVE_BLOCKLIST_ITEMS = 'blocklist/removeBlocklistItems';
export const CLEAR_BLOCKLIST = 'blocklist/clearBlocklist';

//
// Action Creators

export const fetchBlocklist = createThunk(FETCH_BLOCKLIST);
export const gotoBlocklistFirstPage = createThunk(GOTO_FIRST_BLOCKLIST_PAGE);
export const gotoBlocklistPreviousPage = createThunk(GOTO_PREVIOUS_BLOCKLIST_PAGE);
export const gotoBlocklistNextPage = createThunk(GOTO_NEXT_BLOCKLIST_PAGE);
export const gotoBlocklistLastPage = createThunk(GOTO_LAST_BLOCKLIST_PAGE);
export const gotoBlocklistPage = createThunk(GOTO_BLOCKLIST_PAGE);
export const setBlocklistSort = createThunk(SET_BLOCKLIST_SORT);
export const setBlocklistTableOption = createAction(SET_BLOCKLIST_TABLE_OPTION);
export const removeBlocklistItem = createThunk(REMOVE_BLOCKLIST_ITEM);
export const removeBlocklistItems = createThunk(REMOVE_BLOCKLIST_ITEMS);
export const clearBlocklist = createAction(CLEAR_BLOCKLIST);

//
// Action Handlers

export const actionHandlers = handleThunks({
  ...createServerSideCollectionHandlers(
    section,
    '/blocklist',
    fetchBlocklist,
    {
      [serverSideCollectionHandlers.FETCH]: FETCH_BLOCKLIST,
      [serverSideCollectionHandlers.FIRST_PAGE]: GOTO_FIRST_BLOCKLIST_PAGE,
      [serverSideCollectionHandlers.PREVIOUS_PAGE]: GOTO_PREVIOUS_BLOCKLIST_PAGE,
      [serverSideCollectionHandlers.NEXT_PAGE]: GOTO_NEXT_BLOCKLIST_PAGE,
      [serverSideCollectionHandlers.LAST_PAGE]: GOTO_LAST_BLOCKLIST_PAGE,
      [serverSideCollectionHandlers.EXACT_PAGE]: GOTO_BLOCKLIST_PAGE,
      [serverSideCollectionHandlers.SORT]: SET_BLOCKLIST_SORT
    }),

  [REMOVE_BLOCKLIST_ITEM]: createRemoveItemHandler(section, '/blocklist'),

  [REMOVE_BLOCKLIST_ITEMS]: function(getState, payload, dispatch) {
    const {
      ids
    } = payload;

    dispatch(batchActions([
      ...ids.map((id) => {
        return updateItem({
          section,
          id,
          isRemoving: true
        });
      }),

      set({ section, isRemoving: true })
    ]));

    const promise = createAjaxRequest({
      url: '/blocklist/bulk',
      method: 'DELETE',
      dataType: 'json',
      contentType: 'application/json',
      data: JSON.stringify({ ids })
    }).request;

    promise.done((data) => {
      // Don't use batchActions with thunks
      dispatch(fetchBlocklist());

      dispatch(set({ section, isRemoving: false }));
    });

    promise.fail((xhr) => {
      dispatch(batchActions([
        ...ids.map((id) => {
          return updateItem({
            section,
            id,
            isRemoving: false
          });
        }),

        set({ section, isRemoving: false })
      ]));
    });
  }
});

//
// Reducers

export const reducers = createHandleActions({

  [SET_BLOCKLIST_TABLE_OPTION]: createSetTableOptionReducer(section),

  [CLEAR_BLOCKLIST]: createClearReducer(section, {
    isFetching: false,
    isPopulated: false,
    error: null,
    items: [],
    totalPages: 0,
    totalRecords: 0
  })

}, defaultState, section);
