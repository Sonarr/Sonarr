import { handleActions } from 'redux-actions';
import * as types from 'Store/Actions/actionTypes';
import { sortDirections } from 'Helpers/Props';
import createClearReducer from './Creators/createClearReducer';
import createSetReducer from './Creators/createSetReducer';
import createSetTableOptionReducer from './Creators/createSetTableOptionReducer';
import createUpdateReducer from './Creators/createUpdateReducer';
import createUpdateItemReducer from './Creators/createUpdateItemReducer';
import createUpdateServerSideCollectionReducer from './Creators/createUpdateServerSideCollectionReducer';

export const defaultState = {
  isFetching: false,
  isPopulated: false,
  error: null,
  pageSize: 20,
  sortKey: 'date',
  sortDirection: sortDirections.DESCENDING,
  filterKey: null,
  filterValue: null,
  items: [],

  columns: [
    {
      name: 'eventType',
      columnLabel: 'Event Type',
      isVisible: true,
      isModifiable: false
    },
    {
      name: 'series.sortTitle',
      label: 'Series',
      isSortable: true,
      isVisible: true
    },
    {
      name: 'episode',
      label: 'Episode',
      isVisible: true
    },
    {
      name: 'episodeTitle',
      label: 'Episode Title',
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
      name: 'downloadClient',
      label: 'Download Client',
      isVisible: false
    },
    {
      name: 'indexer',
      label: 'Indexer',
      isVisible: false
    },
    {
      name: 'releaseGroup',
      label: 'Release Group',
      isVisible: false
    },
    {
      name: 'details',
      columnLabel: 'Details',
      isVisible: true,
      isModifiable: false
    }
  ]
};

export const persistState = [
  'history.pageSize',
  'history.sortKey',
  'history.sortDirection',
  'history.filterKey',
  'history.filterValue'
];

const serverSideCollectionName = 'history';

const historyReducers = handleActions({

  [types.SET]: createSetReducer(serverSideCollectionName),
  [types.UPDATE]: createUpdateReducer(serverSideCollectionName),
  [types.UPDATE_ITEM]: createUpdateItemReducer(serverSideCollectionName),
  [types.UPDATE_SERVER_SIDE_COLLECTION]: createUpdateServerSideCollectionReducer(serverSideCollectionName),

  [types.SET_HISTORY_TABLE_OPTION]: createSetTableOptionReducer(serverSideCollectionName),

  [types.CLEAR_HISTORY]: createClearReducer('history', {
    isFetching: false,
    isPopulated: false,
    error: null,
    items: []
  })

}, defaultState);

export default historyReducers;
