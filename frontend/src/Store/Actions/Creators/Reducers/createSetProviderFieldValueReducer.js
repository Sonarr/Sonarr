import getSectionState from 'Utilities/State/getSectionState';
import updateSectionState from 'Utilities/State/updateSectionState';

function createSetProviderFieldValueReducer(section) {
  return (state, { payload }) => {
    if (section === payload.section) {
      const { name, value } = payload;
      const newState = getSectionState(state, section);
      newState.pendingChanges = Object.assign({}, newState.pendingChanges);
      const fields = Object.assign({}, newState.pendingChanges.fields || {});

      fields[name] = value;

      newState.pendingChanges.fields = fields;

      return updateSectionState(state, section, newState);
    }

    return state;
  };
}

export default createSetProviderFieldValueReducer;
