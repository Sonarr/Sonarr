import _ from 'lodash';
import getSectionState from 'Utilities/State/getSectionState';
import updateSectionState from 'Utilities/State/updateSectionState';

function createUpdateReducer(section) {
  return (state, { payload }) => {
    if (section === payload.section) {
      const newState = getSectionState(state, section);

      if (_.isArray(payload.data)) {
        newState.items = payload.data;
      } else {
        newState.item = payload.data;
      }

      return updateSectionState(state, section, newState);
    }

    return state;
  };
}

export default createUpdateReducer;
