import getSectionState from 'Utilities/State/getSectionState';
import updateSectionState from 'Utilities/State/updateSectionState';

function createSetClientSideCollectionFilterReducer(section) {
  return (state, { payload }) => {
    const newState = getSectionState(state, section);

    newState.selectedFilterKey = payload.selectedFilterKey;

    return updateSectionState(state, section, newState);
  };
}

export default createSetClientSideCollectionFilterReducer;
