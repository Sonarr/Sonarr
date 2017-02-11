import { handleActions } from 'redux-actions';
import { sortDirections } from 'Helpers/Props';
import * as types from 'Store/Actions/actionTypes';
import createClearReducer from './Creators/createClearReducer';
import createSetReducer from './Creators/createSetReducer';
import createSetTableOptionReducer from './Creators/createSetTableOptionReducer';
import createUpdateReducer from './Creators/createUpdateReducer';
import createUpdateItemReducer from './Creators/createUpdateItemReducer';
import createReducers from './Creators/createReducers';
import createUpdateServerSideCollectionReducer from './Creators/createUpdateServerSideCollectionReducer';

export const defaultState = {
  queueStatus: {
    isFetching: false,
    isPopulated: false,
    error: null,
    item: {}
  },

  details: {
    isFetching: false,
    isPopulated: false,
    error: null,
    items: [],
    params: {}
  },

  paged: {
    isFetching: false,
    isPopulated: false,
    pageSize: 20,
    sortKey: 'timeleft',
    sortDirection: sortDirections.ASCENDING,
    error: null,
    items: [],
    isGrabbing: false,
    isRemoving: false,

    columns: [
      {
        name: 'status',
        columnLabel: 'Status',
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
        name: 'quality',
        label: 'Quality',
        isSortable: true,
        isVisible: true
      },
      {
        name: 'protocol',
        label: 'Protocol',
        isVisible: false
      },
      {
        name: 'indexer',
        label: 'Indexer',
        isVisible: false
      },
      {
        name: 'downloadClient',
        label: 'Download Client',
        isVisible: false
      },
      {
        name: 'estimatedCompletionTime',
        label: 'Timeleft',
        isSortable: true,
        isVisible: true
      },
      {
        name: 'progress',
        label: 'Progress',
        isSortable: true,
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

  queueEpisodes: {
    isPopulated: false,
    items: []
  }
};

export const persistState = [
  'queue.paged.pageSize',
  'queue.paged.sortKey',
  'queue.paged.sortDirection',
  'queue.paged.columns'
];

const propertyNames = [
  'queueStatus',
  'details',
  'episodes'
];

const paged = 'paged';

const queueReducers = handleActions({

  [types.SET]: createReducers([...propertyNames, paged], createSetReducer),
  [types.UPDATE]: createReducers([...propertyNames, paged], createUpdateReducer),
  [types.UPDATE_ITEM]: createReducers(['queueEpisodes', paged], createUpdateItemReducer),

  [types.CLEAR_QUEUE_DETAILS]: createClearReducer('details', defaultState.details),

  [types.UPDATE_SERVER_SIDE_COLLECTION]: createUpdateServerSideCollectionReducer(paged),

  [types.SET_QUEUE_TABLE_OPTION]: createSetTableOptionReducer(paged),

  [types.CLEAR_QUEUE]: createClearReducer('paged', {
    isFetching: false,
    isPopulated: false,
    error: null,
    items: []
  })

}, defaultState);

export default queueReducers;
