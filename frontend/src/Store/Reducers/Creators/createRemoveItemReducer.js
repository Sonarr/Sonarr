import _ from 'lodash';
import getSectionState from 'Utilities/State/getSectionState';
import updateSectionState from 'Utilities/State/updateSectionState';

function createRemoveItemReducer(section) {
  return (state, { payload }) => {
    if (section === payload.section) {
      const newState = getSectionState(state, section);

      newState.items = [...newState.items];
      _.remove(newState.items, { id: payload.id });

      return updateSectionState(state, section, newState);
    }

    return state;
  };
}

export default createRemoveItemReducer;
