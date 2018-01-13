import getSectionState from 'Utilities/State/getSectionState';
import updateSectionState from 'Utilities/State/updateSectionState';

function createClearReducer(section, defaultState) {
  return (state) => {
    const newState = Object.assign(getSectionState(state, section), defaultState);

    return updateSectionState(state, section, newState);
  };
}

export default createClearReducer;
