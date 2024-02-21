import createFetchHandler from 'Store/Actions/Creators/createFetchHandler';
import { createThunk } from 'Store/thunks';

//
// Variables

const section = 'settings.indexerFlags';

//
// Actions Types

export const FETCH_INDEXER_FLAGS = 'settings/indexerFlags/fetchIndexerFlags';

//
// Action Creators

export const fetchIndexerFlags = createThunk(FETCH_INDEXER_FLAGS);

//
// Details

export default {

  //
  // State

  defaultState: {
    isFetching: false,
    isPopulated: false,
    error: null,
    items: []
  },

  //
  // Action Handlers

  actionHandlers: {
    [FETCH_INDEXER_FLAGS]: createFetchHandler(section, '/indexerFlag')
  },

  //
  // Reducers

  reducers: {

  }

};
