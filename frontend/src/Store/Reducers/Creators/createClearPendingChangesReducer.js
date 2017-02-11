import getSectionState from 'Utilities/State/getSectionState';
import updateSectionState from 'Utilities/State/updateSectionState';

function createClearPendingChangesReducer(section) {
  return (state, { payload }) => {
    if (section === payload.section) {
      const newState = getSectionState(state, section);
      newState.pendingChanges = {};

      if (newState.hasOwnProperty('saveError')) {
        newState.saveError = null;
      }

      return updateSectionState(state, section, newState);
    }

    return state;
  };
}

export default createClearPendingChangesReducer;
