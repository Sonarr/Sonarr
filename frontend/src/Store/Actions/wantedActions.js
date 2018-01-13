import { createAction } from 'redux-actions';
import serverSideCollectionHandlers from 'Utilities/serverSideCollectionHandlers';
import { filterTypes, sortDirections } from 'Helpers/Props';
import { createThunk, handleThunks } from 'Store/thunks';
import createClearReducer from './Creators/Reducers/createClearReducer';
import createSetTableOptionReducer from './Creators/Reducers/createSetTableOptionReducer';
import createBatchToggleEpisodeMonitoredHandler from './Creators/createBatchToggleEpisodeMonitoredHandler';
import createServerSideCollectionHandlers from './Creators/createServerSideCollectionHandlers';
import createHandleActions from './Creators/createHandleActions';

//
// Variables

export const section = 'wanted';

//
// State

export const defaultState = {
  missing: {
    isFetching: false,
    isPopulated: false,
    pageSize: 20,
    sortKey: 'airDateUtc',
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
    ],

    selectedFilterKey: 'monitored',

    filters: [
      {
        key: 'monitored',
        label: 'Monitored',
        filters: [
          {
            key: 'monitored',
            value: true,
            type: filterTypes.EQUAL
          }
        ]
      },
      {
        key: 'unmonitored',
        label: 'Unmonitored',
        filters: [
          {
            key: 'monitored',
            value: false,
            type: filterTypes.EQUAL
          }
        ]
      }
    ]
  },

  cutoffUnmet: {
    isFetching: false,
    isPopulated: false,
    pageSize: 20,
    sortKey: 'airDateUtc',
    sortDirection: sortDirections.DESCENDING,
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
    ],

    selectedFilterKey: 'monitored',

    filters: [
      {
        key: 'monitored',
        label: 'Monitored',
        filters: [
          {
            key: 'monitored',
            value: true,
            type: filterTypes.EQUAL
          }
        ]
      },
      {
        key: 'unmonitored',
        label: 'Unmonitored',
        filters: [
          {
            key: 'monitored',
            value: false,
            type: filterTypes.EQUAL
          }
        ]
      }
    ]
  }
};

export const persistState = [
  'wanted.missing.pageSize',
  'wanted.missing.sortKey',
  'wanted.missing.sortDirection',
  'wanted.missing.selectedFilterKey',
  'wanted.missing.columns',
  'wanted.cutoffUnmet.pageSize',
  'wanted.cutoffUnmet.sortKey',
  'wanted.cutoffUnmet.sortDirection',
  'wanted.cutoffUnmet.selectedFilterKey',
  'wanted.cutoffUnmet.columns'
];

//
// Actions Types

export const FETCH_MISSING = 'wanted/missing/fetchMissing';
export const GOTO_FIRST_MISSING_PAGE = 'wanted/missing/gotoMissingFirstPage';
export const GOTO_PREVIOUS_MISSING_PAGE = 'wanted/missing/gotoMissingPreviousPage';
export const GOTO_NEXT_MISSING_PAGE = 'wanted/missing/gotoMissingNextPage';
export const GOTO_LAST_MISSING_PAGE = 'wanted/missing/gotoMissingLastPage';
export const GOTO_MISSING_PAGE = 'wanted/missing/gotoMissingPage';
export const SET_MISSING_SORT = 'wanted/missing/setMissingSort';
export const SET_MISSING_FILTER = 'wanted/missing/setMissingFilter';
export const SET_MISSING_TABLE_OPTION = 'wanted/missing/setMissingTableOption';
export const CLEAR_MISSING = 'wanted/missing/clearMissing';

export const BATCH_TOGGLE_MISSING_EPISODES = 'wanted/missing/';

export const FETCH_CUTOFF_UNMET = 'wanted/cutoffUnmet/fetchCutoffUnmet';
export const GOTO_FIRST_CUTOFF_UNMET_PAGE = 'wanted/cutoffUnmet/gotoCutoffUnmetFirstPage';
export const GOTO_PREVIOUS_CUTOFF_UNMET_PAGE = 'wanted/cutoffUnmet/gotoCutoffUnmetPreviousPage';
export const GOTO_NEXT_CUTOFF_UNMET_PAGE = 'wanted/cutoffUnmet/gotoCutoffUnmetNextPage';
export const GOTO_LAST_CUTOFF_UNMET_PAGE = 'wanted/cutoffUnmet/gotoCutoffUnmetFastPage';
export const GOTO_CUTOFF_UNMET_PAGE = 'wanted/cutoffUnmet/gotoCutoffUnmetPage';
export const SET_CUTOFF_UNMET_SORT = 'wanted/cutoffUnmet/setCutoffUnmetSort';
export const SET_CUTOFF_UNMET_FILTER = 'wanted/cutoffUnmet/setCutoffUnmetFilter';
export const SET_CUTOFF_UNMET_TABLE_OPTION = 'wanted/cutoffUnmet/setCutoffUnmetTableOption';
export const CLEAR_CUTOFF_UNMET = 'wanted/cutoffUnmet/clearCutoffUnmet';

export const BATCH_TOGGLE_CUTOFF_UNMET_EPISODES = 'wanted/cutoffUnmet/batchToggleCutoffUnmetEpisodes';

//
// Action Creators

export const fetchMissing = createThunk(FETCH_MISSING);
export const gotoMissingFirstPage = createThunk(GOTO_FIRST_MISSING_PAGE);
export const gotoMissingPreviousPage = createThunk(GOTO_PREVIOUS_MISSING_PAGE);
export const gotoMissingNextPage = createThunk(GOTO_NEXT_MISSING_PAGE);
export const gotoMissingLastPage = createThunk(GOTO_LAST_MISSING_PAGE);
export const gotoMissingPage = createThunk(GOTO_MISSING_PAGE);
export const setMissingSort = createThunk(SET_MISSING_SORT);
export const setMissingFilter = createThunk(SET_MISSING_FILTER);
export const setMissingTableOption = createAction(SET_MISSING_TABLE_OPTION);
export const clearMissing = createAction(CLEAR_MISSING);

export const batchToggleMissingEpisodes = createThunk(BATCH_TOGGLE_MISSING_EPISODES);

export const fetchCutoffUnmet = createThunk(FETCH_CUTOFF_UNMET);
export const gotoCutoffUnmetFirstPage = createThunk(GOTO_FIRST_CUTOFF_UNMET_PAGE);
export const gotoCutoffUnmetPreviousPage = createThunk(GOTO_PREVIOUS_CUTOFF_UNMET_PAGE);
export const gotoCutoffUnmetNextPage = createThunk(GOTO_NEXT_CUTOFF_UNMET_PAGE);
export const gotoCutoffUnmetLastPage = createThunk(GOTO_LAST_CUTOFF_UNMET_PAGE);
export const gotoCutoffUnmetPage = createThunk(GOTO_CUTOFF_UNMET_PAGE);
export const setCutoffUnmetSort = createThunk(SET_CUTOFF_UNMET_SORT);
export const setCutoffUnmetFilter = createThunk(SET_CUTOFF_UNMET_FILTER);
export const setCutoffUnmetTableOption = createAction(SET_CUTOFF_UNMET_TABLE_OPTION);
export const clearCutoffUnmet = createAction(CLEAR_CUTOFF_UNMET);

export const batchToggleCutoffUnmetEpisodes = createThunk(BATCH_TOGGLE_CUTOFF_UNMET_EPISODES);

//
// Action Handlers

export const actionHandlers = handleThunks({

  ...createServerSideCollectionHandlers(
    'wanted.missing',
    '/wanted/missing',
    fetchMissing,
    {
      [serverSideCollectionHandlers.FETCH]: FETCH_MISSING,
      [serverSideCollectionHandlers.FIRST_PAGE]: GOTO_FIRST_MISSING_PAGE,
      [serverSideCollectionHandlers.PREVIOUS_PAGE]: GOTO_PREVIOUS_MISSING_PAGE,
      [serverSideCollectionHandlers.NEXT_PAGE]: GOTO_NEXT_MISSING_PAGE,
      [serverSideCollectionHandlers.LAST_PAGE]: GOTO_LAST_MISSING_PAGE,
      [serverSideCollectionHandlers.EXACT_PAGE]: GOTO_MISSING_PAGE,
      [serverSideCollectionHandlers.SORT]: SET_MISSING_SORT,
      [serverSideCollectionHandlers.FILTER]: SET_MISSING_FILTER
    }
  ),

  [BATCH_TOGGLE_MISSING_EPISODES]: createBatchToggleEpisodeMonitoredHandler('wanted.missing', fetchMissing),

  ...createServerSideCollectionHandlers(
    'wanted.cutoffUnmet',
    '/wanted/cutoff',
    fetchCutoffUnmet,
    {
      [serverSideCollectionHandlers.FETCH]: FETCH_CUTOFF_UNMET,
      [serverSideCollectionHandlers.FIRST_PAGE]: GOTO_FIRST_CUTOFF_UNMET_PAGE,
      [serverSideCollectionHandlers.PREVIOUS_PAGE]: GOTO_PREVIOUS_CUTOFF_UNMET_PAGE,
      [serverSideCollectionHandlers.NEXT_PAGE]: GOTO_NEXT_CUTOFF_UNMET_PAGE,
      [serverSideCollectionHandlers.LAST_PAGE]: GOTO_LAST_CUTOFF_UNMET_PAGE,
      [serverSideCollectionHandlers.EXACT_PAGE]: GOTO_CUTOFF_UNMET_PAGE,
      [serverSideCollectionHandlers.SORT]: SET_CUTOFF_UNMET_SORT,
      [serverSideCollectionHandlers.FILTER]: SET_CUTOFF_UNMET_FILTER
    }
  ),

  [BATCH_TOGGLE_CUTOFF_UNMET_EPISODES]: createBatchToggleEpisodeMonitoredHandler('wanted.cutoffUnmet', fetchCutoffUnmet)

});

//
// Reducers

export const reducers = createHandleActions({

  [SET_MISSING_TABLE_OPTION]: createSetTableOptionReducer('wanted.missing'),
  [SET_CUTOFF_UNMET_TABLE_OPTION]: createSetTableOptionReducer('wanted.cutoffUnmet'),

  [CLEAR_MISSING]: createClearReducer(
    'wanted.missing',
    {
      isFetching: false,
      isPopulated: false,
      error: null,
      items: []
    }
  ),

  [CLEAR_CUTOFF_UNMET]: createClearReducer(
    'wanted.cutoffUnmet',
    {
      isFetching: false,
      isPopulated: false,
      error: null,
      items: []
    }
  )

}, defaultState, section);
