import { createAction } from 'redux-actions';
import { sortDirections } from 'Helpers/Props';
import { createThunk, handleThunks } from 'Store/thunks';
import updateSectionState from 'Utilities/State/updateSectionState';
import createFetchHandler from './Creators/createFetchHandler';
import createHandleActions from './Creators/createHandleActions';
import createSetClientSideCollectionSortReducer from './Creators/Reducers/createSetClientSideCollectionSortReducer';

//
// Variables

export const section = 'episodeSelection';

//
// State

export const defaultState = {
  isFetching: false,
  isReprocessing: false,
  isPopulated: false,
  error: null,
  sortKey: 'episodeNumber',
  sortDirection: sortDirections.ASCENDING,
  items: []
};

//
// Actions Types

export const FETCH_EPISODES = 'episodeSelection/fetchEpisodes';
export const SET_EPISODES_SORT = 'episodeSelection/setEpisodesSort';
export const CLEAR_EPISODES = 'episodeSelection/clearEpisodes';

//
// Action Creators

export const fetchEpisodes = createThunk(FETCH_EPISODES);
export const setEpisodesSort = createAction(SET_EPISODES_SORT);
export const clearEpisodes = createAction(CLEAR_EPISODES);

//
// Action Handlers

export const actionHandlers = handleThunks({
  [FETCH_EPISODES]: createFetchHandler(section, '/episode')
});

//
// Reducers

export const reducers = createHandleActions({

  [SET_EPISODES_SORT]: createSetClientSideCollectionSortReducer(section),

  [CLEAR_EPISODES]: (state) => {
    return updateSectionState(state, section, {
      ...defaultState
    });
  }

}, defaultState, section);
