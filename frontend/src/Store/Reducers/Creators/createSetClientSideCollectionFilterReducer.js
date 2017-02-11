import getSectionState from 'Utilities/State/getSectionState';
import updateSectionState from 'Utilities/State/updateSectionState';
import { filterTypes } from 'Helpers/Props';

function createSetClientSideCollectionFilterReducer(section) {
  return (state, { payload }) => {
    const newState = getSectionState(state, section);

    newState.filterKey = payload.filterKey;
    newState.filterValue = payload.filterValue;
    newState.filterType = payload.filterType || filterTypes.EQUAL;

    return updateSectionState(state, section, newState);
  };
}

export default createSetClientSideCollectionFilterReducer;
