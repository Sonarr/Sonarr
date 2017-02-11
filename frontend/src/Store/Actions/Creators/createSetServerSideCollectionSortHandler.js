import { sortDirections } from 'Helpers/Props';
import { set } from '../baseActions';

function createSetServerSideCollectionSortHandler(section, getFromState, fetchHandler) {
  return function(payload) {
    return function(dispatch, getState) {
      const state = getFromState(getState());
      const sectionState = state.hasOwnProperty(section) ? state[section] : state;
      const sortKey = payload.sortKey || sectionState.sortKey;
      let sortDirection = payload.sortDirection;

      if (!sortDirection) {
        if (payload.sortKey === sectionState.sortKey) {
          sortDirection = sectionState.sortDirection === sortDirections.ASCENDING ?
            sortDirections.DESCENDING :
            sortDirections.ASCENDING;
        } else {
          sortDirection = sectionState.sortDirection;
        }
      }

      dispatch(set({ section, sortKey, sortDirection }));
      dispatch(fetchHandler());
    };
  };
}

export default createSetServerSideCollectionSortHandler;
