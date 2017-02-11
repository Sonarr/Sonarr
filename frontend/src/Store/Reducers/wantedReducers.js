import { handleActions } from 'redux-actions';
import * as types from 'Store/Actions/actionTypes';
import { sortDirections } from 'Helpers/Props';
import createClearReducer from './Creators/createClearReducer';
import createSetReducer from './Creators/createSetReducer';
import createSetTableOptionReducer from './Creators/createSetTableOptionReducer';
import createUpdateReducer from './Creators/createUpdateReducer';
import createUpdateItemReducer from './Creators/createUpdateItemReducer';
import createUpdateServerSideCollectionReducer from './Creators/createUpdateServerSideCollectionReducer';
import createReducers from './Creators/createReducers';

export const defaultState = {
  missing: {
    isFetching: false,
    isPopulated: false,
    pageSize: 20,
    sortKey: 'airDateUtc',
    sortDirection: sortDirections.DESCENDING,
    filterKey: 'monitored',
    filterValue: 'true',
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
        name: 'airDateUtc',
        label: 'Air Date',
        isSortable: true,
        isVisible: true
      },
      {
        name: 'status',
        label: 'Status',
        isVisible: true
      },
      {
        name: 'actions',
        columnLabel: 'Actions',
        isVisible: true,
        isModifiable: false
      }
    ]
  },

  cutoffUnmet: {
    isFetching: false,
    isPopulated: false,
    pageSize: 20,
    sortKey: 'airDateUtc',
    sortDirection: sortDirections.DESCENDING,
    filterKey: 'monitored',
    filterValue: true,
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
        name: 'airDateUtc',
        label: 'Air Date',
        isSortable: true,
        isVisible: true
      },
      {
        name: 'language',
        label: 'Language',
        isVisible: false
      },
      {
        name: 'status',
        label: 'Status',
        isVisible: true
      },
      {
        name: 'actions',
        columnLabel: 'Actions',
        isVisible: true,
        isModifiable: false
      }
    ]
  }
};

export const persistState = [
  'wanted.missing.pageSize',
  'wanted.missing.sortKey',
  'wanted.missing.sortDirection',
  'wanted.missing.filterKey',
  'wanted.missing.filterValue',
  'wanted.missing.columns',
  'wanted.cutoffUnmet.pageSize',
  'wanted.cutoffUnmet.sortKey',
  'wanted.cutoffUnmet.sortDirection',
  'wanted.cutoffUnmet.filterKey',
  'wanted.cutoffUnmet.filterValue',
  'wanted.cutoffUnmet.columns'
];

const serverSideCollectionNames = [
  'missing',
  'cutoffUnmet'
];

const wantedReducers = handleActions({

  [types.SET]: createReducers(serverSideCollectionNames, createSetReducer),
  [types.UPDATE]: createReducers(serverSideCollectionNames, createUpdateReducer),
  [types.UPDATE_ITEM]: createReducers(serverSideCollectionNames, createUpdateItemReducer),
  [types.UPDATE_SERVER_SIDE_COLLECTION]: createReducers(serverSideCollectionNames, createUpdateServerSideCollectionReducer),

  [types.SET_MISSING_TABLE_OPTION]: createSetTableOptionReducer('missing'),
  [types.SET_CUTOFF_UNMET_TABLE_OPTION]: createSetTableOptionReducer('cutoffUnmet'),

  [types.CLEAR_MISSING]: createClearReducer('missing', {
    isFetching: false,
    isPopulated: false,
    error: null,
    items: []
  }),

  [types.CLEAR_CUTOFF_UNMET]: createClearReducer('cutoffUnmet', {
    isFetching: false,
    isPopulated: false,
    error: null,
    items: []
  })

}, defaultState);

export default wantedReducers;
