import _ from 'lodash';
import getSectionState from 'Utilities/State/getSectionState';
import updateSectionState from 'Utilities/State/updateSectionState';

function createSetSettingValueReducer(section) {
  return (state, { payload }) => {
    if (section === payload.section) {
      const { name, value } = payload;
      const newState = getSectionState(state, section);
      newState.pendingChanges = Object.assign({}, newState.pendingChanges);

      const currentValue = newState.item ? newState.item[name] : null;
      const pendingState = newState.pendingChanges;

      let parsedValue = null;

      if (_.isNumber(currentValue)) {
        parsedValue = parseInt(value);
      } else {
        parsedValue = value;
      }

      if (currentValue === parsedValue) {
        delete pendingState[name];
      } else {
        pendingState[name] = parsedValue;
      }

      return updateSectionState(state, section, newState);
    }

    return state;
  };
}

export default createSetSettingValueReducer;
