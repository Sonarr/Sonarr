import { sortDirections } from 'Helpers/Props';
import getSectionState from 'Utilities/State/getSectionState';
import { set } from '../baseActions';

function createSetServerSideCollectionSortHandler(section, fetchHandler) {
  return function(getState, payload, dispatch) {
    const sectionState = getSectionState(getState(), section, true);
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
}

export default createSetServerSideCollectionSortHandler;
