import getSectionState from 'Utilities/State/getSectionState';
import updateSectionState from 'Utilities/State/updateSectionState';

function createSetProviderFieldValuesReducer(section) {
  return (state, { payload }) => {
    if (section === payload.section) {
      const { properties } = payload;
      const newState = getSectionState(state, section);
      newState.pendingChanges = Object.assign({}, newState.pendingChanges);
      const fields = Object.assign({}, newState.pendingChanges.fields || {});

      Object.keys(properties).forEach((name) => {
        fields[name] = properties[name];
      });

      newState.pendingChanges.fields = fields;

      return updateSectionState(state, section, newState);
    }

    return state;
  };
}

export default createSetProviderFieldValuesReducer;
