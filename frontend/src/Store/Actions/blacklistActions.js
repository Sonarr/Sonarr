import { createAction } from 'redux-actions';
import serverSideCollectionHandlers from 'Utilities/serverSideCollectionHandlers';
import { createThunk, handleThunks } from 'Store/thunks';
import { sortDirections } from 'Helpers/Props';
import createSetTableOptionReducer from './Creators/Reducers/createSetTableOptionReducer';
import createHandleActions from './Creators/createHandleActions';
import createRemoveItemHandler from './Creators/createRemoveItemHandler';
import createServerSideCollectionHandlers from './Creators/createServerSideCollectionHandlers';

//
// Variables

export const section = 'blacklist';

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
  'blacklist.pageSize',
  'blacklist.sortKey',
  'blacklist.sortDirection',
  'blacklist.columns'
];

//
// Action Types

export const FETCH_BLACKLIST = 'blacklist/fetchBlacklist';
export const GOTO_FIRST_BLACKLIST_PAGE = 'blacklist/gotoBlacklistFirstPage';
export const GOTO_PREVIOUS_BLACKLIST_PAGE = 'blacklist/gotoBlacklistPreviousPage';
export const GOTO_NEXT_BLACKLIST_PAGE = 'blacklist/gotoBlacklistNextPage';
export const GOTO_LAST_BLACKLIST_PAGE = 'blacklist/gotoBlacklistLastPage';
export const GOTO_BLACKLIST_PAGE = 'blacklist/gotoBlacklistPage';
export const SET_BLACKLIST_SORT = 'blacklist/setBlacklistSort';
export const SET_BLACKLIST_TABLE_OPTION = 'blacklist/setBlacklistTableOption';
export const REMOVE_FROM_BLACKLIST = 'blacklist/removeFromBlacklist';

//
// Action Creators

export const fetchBlacklist = createThunk(FETCH_BLACKLIST);
export const gotoBlacklistFirstPage = createThunk(GOTO_FIRST_BLACKLIST_PAGE);
export const gotoBlacklistPreviousPage = createThunk(GOTO_PREVIOUS_BLACKLIST_PAGE);
export const gotoBlacklistNextPage = createThunk(GOTO_NEXT_BLACKLIST_PAGE);
export const gotoBlacklistLastPage = createThunk(GOTO_LAST_BLACKLIST_PAGE);
export const gotoBlacklistPage = createThunk(GOTO_BLACKLIST_PAGE);
export const setBlacklistSort = createThunk(SET_BLACKLIST_SORT);
export const setBlacklistTableOption = createAction(SET_BLACKLIST_TABLE_OPTION);
export const removeFromBlacklist = createThunk(REMOVE_FROM_BLACKLIST);

//
// Action Handlers

export const actionHandlers = handleThunks({
  ...createServerSideCollectionHandlers(
    section,
    '/blacklist',
    fetchBlacklist,
    {
      [serverSideCollectionHandlers.FETCH]: FETCH_BLACKLIST,
      [serverSideCollectionHandlers.FIRST_PAGE]: GOTO_FIRST_BLACKLIST_PAGE,
      [serverSideCollectionHandlers.PREVIOUS_PAGE]: GOTO_PREVIOUS_BLACKLIST_PAGE,
      [serverSideCollectionHandlers.NEXT_PAGE]: GOTO_NEXT_BLACKLIST_PAGE,
      [serverSideCollectionHandlers.LAST_PAGE]: GOTO_LAST_BLACKLIST_PAGE,
      [serverSideCollectionHandlers.EXACT_PAGE]: GOTO_BLACKLIST_PAGE,
      [serverSideCollectionHandlers.SORT]: SET_BLACKLIST_SORT
    }),

  [REMOVE_FROM_BLACKLIST]: createRemoveItemHandler(section, '/blacklist')
});

//
// Reducers

export const reducers = createHandleActions({

  [SET_BLACKLIST_TABLE_OPTION]: createSetTableOptionReducer(section)

}, defaultState, section);
