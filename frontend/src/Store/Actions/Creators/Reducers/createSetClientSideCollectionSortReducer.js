import { sortDirections } from 'Helpers/Props';
import getSectionState from 'Utilities/State/getSectionState';
import updateSectionState from 'Utilities/State/updateSectionState';

function createSetClientSideCollectionSortReducer(section) {
  return (state, { payload }) => {
    const newState = getSectionState(state, section);

    const sortKey = payload.sortKey || newState.sortKey;
    let sortDirection = payload.sortDirection;

    if (!sortDirection) {
      if (payload.sortKey === newState.sortKey) {
        sortDirection = newState.sortDirection === sortDirections.ASCENDING ?
          sortDirections.DESCENDING :
          sortDirections.ASCENDING;
      } else {
        sortDirection = newState.sortDirection;
      }
    }

    newState.sortKey = sortKey;
    newState.sortDirection = sortDirection;

    return updateSectionState(state, section, newState);
  };
}

export default createSetClientSideCollectionSortReducer;
