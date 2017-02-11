import serverSideCollectionHandlers from 'Utilities/serverSideCollectionHandlers';
import createBatchToggleEpisodeMonitoredHandler from './Creators/createBatchToggleEpisodeMonitoredHandler';
import createServerSideCollectionHandlers from './Creators/createServerSideCollectionHandlers';
import * as types from './actionTypes';

const wantedActionHandlers = {
  ...createServerSideCollectionHandlers('missing', '/wanted/missing', (state) => state.wanted, {
    [serverSideCollectionHandlers.FETCH]: types.FETCH_MISSING,
    [serverSideCollectionHandlers.FIRST_PAGE]: types.GOTO_FIRST_MISSING_PAGE,
    [serverSideCollectionHandlers.PREVIOUS_PAGE]: types.GOTO_PREVIOUS_MISSING_PAGE,
    [serverSideCollectionHandlers.NEXT_PAGE]: types.GOTO_NEXT_MISSING_PAGE,
    [serverSideCollectionHandlers.LAST_PAGE]: types.GOTO_LAST_MISSING_PAGE,
    [serverSideCollectionHandlers.EXACT_PAGE]: types.GOTO_MISSING_PAGE,
    [serverSideCollectionHandlers.SORT]: types.SET_MISSING_SORT,
    [serverSideCollectionHandlers.FILTER]: types.SET_MISSING_FILTER
  }),

  [types.BATCH_TOGGLE_MISSING_EPISODES]: createBatchToggleEpisodeMonitoredHandler('missing', (state) => state.wanted.missing),

  ...createServerSideCollectionHandlers('cutoffUnmet', '/wanted/cutoff', (state) => state.wanted, {
    [serverSideCollectionHandlers.FETCH]: types.FETCH_CUTOFF_UNMET,
    [serverSideCollectionHandlers.FIRST_PAGE]: types.GOTO_FIRST_CUTOFF_UNMET_PAGE,
    [serverSideCollectionHandlers.PREVIOUS_PAGE]: types.GOTO_PREVIOUS_CUTOFF_UNMET_PAGE,
    [serverSideCollectionHandlers.NEXT_PAGE]: types.GOTO_NEXT_CUTOFF_UNMET_PAGE,
    [serverSideCollectionHandlers.LAST_PAGE]: types.GOTO_LAST_CUTOFF_UNMET_PAGE,
    [serverSideCollectionHandlers.EXACT_PAGE]: types.GOTO_CUTOFF_UNMET_PAGE,
    [serverSideCollectionHandlers.SORT]: types.SET_CUTOFF_UNMET_SORT,
    [serverSideCollectionHandlers.FILTER]: types.SET_CUTOFF_UNMET_FILTER
  }),

  [types.BATCH_TOGGLE_CUTOFF_UNMET_EPISODES]: createBatchToggleEpisodeMonitoredHandler('cutoffUnmet', (state) => state.wanted.cutoffUnmet)
};

export default wantedActionHandlers;
