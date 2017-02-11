import { handleActions } from 'redux-actions';
import * as types from 'Store/Actions/actionTypes';
import { sortDirections } from 'Helpers/Props';
import createSetReducer from './Creators/createSetReducer';
import createSetTableOptionReducer from './Creators/createSetTableOptionReducer';
import createUpdateReducer from './Creators/createUpdateReducer';
import createUpdateItemReducer from './Creators/createUpdateItemReducer';
import createUpdateServerSideCollectionReducer from './Creators/createUpdateServerSideCollectionReducer';
import createReducers from './Creators/createReducers';

export const defaultState = {
  status: {
    isFetching: false,
    isPopulated: false,
    error: null,
    item: {}
  },

  health: {
    isFetching: false,
    isPopulated: false,
    error: null,
    items: []
  },

  diskSpace: {
    isFetching: false,
    isPopulated: false,
    error: null,
    items: []
  },

  tasks: {
    isFetching: false,
    isPopulated: false,
    error: null,
    items: []
  },

  backups: {
    isFetching: false,
    isPopulated: false,
    error: null,
    items: []
  },

  updates: {
    isFetching: false,
    isPopulated: false,
    error: null,
    items: []
  },

  logs: {
    isFetching: false,
    isPopulated: false,
    pageSize: 50,
    sortKey: 'time',
    sortDirection: sortDirections.DESCENDING,
    filterKey: null,
    filterValue: null,
    error: null,
    items: [],

    columns: [
      {
        name: 'level',
        isSortable: true,
        isVisible: true
      },
      {
        name: 'logger',
        label: 'Component',
        isSortable: true,
        isVisible: true
      },
      {
        name: 'message',
        label: 'Message',
        isVisible: true
      },
      {
        name: 'time',
        label: 'Time',
        isSortable: true,
        isVisible: true
      },
      {
        name: 'actions',
        columnLabel: 'Actions',
        isSortable: true,
        isVisible: true,
        isModifiable: false
      }
    ]
  },

  logFiles: {
    isFetching: false,
    isPopulated: false,
    error: null,
    items: []
  },

  updateLogFiles: {
    isFetching: false,
    isPopulated: false,
    error: null,
    items: []
  }
};

export const persistState = [
  'system.logs.pageSize',
  'system.logs.sortKey',
  'system.logs.sortDirection',
  'system.logs.filterKey',
  'system.logs.filterValue'
];

const collectionNames = [
  'health',
  'diskSpace',
  'tasks',
  'backups',
  'updates',
  'logFiles',
  'updateLogFiles'
];

const serverSideCollectionNames = [
  'logs'
];

const systemReducers = handleActions({

  [types.SET]: createReducers(['status', ...collectionNames, ...serverSideCollectionNames], createSetReducer),
  [types.UPDATE]: createReducers(['status', ...collectionNames, ...serverSideCollectionNames], createUpdateReducer),
  [types.UPDATE_ITEM]: createUpdateItemReducer('tasks'),
  [types.UPDATE_SERVER_SIDE_COLLECTION]: createReducers(serverSideCollectionNames, createUpdateServerSideCollectionReducer),

  [types.SET_LOGS_TABLE_OPTION]: createSetTableOptionReducer('logs')

}, defaultState);

export default systemReducers;
