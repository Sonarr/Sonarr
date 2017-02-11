import getSectionState from 'Utilities/State/getSectionState';
import updateSectionState from 'Utilities/State/updateSectionState';

function createAddItemReducer(section) {
  return (state, { payload }) => {
    const {
      section: payloadSection,
      ...otherProps
    } = payload;

    if (section === payloadSection) {
      const newState = getSectionState(state, section);

      newState.items = [...newState.items, { ...otherProps }];

      return updateSectionState(state, section, newState);
    }

    return state;
  };
}

export default createAddItemReducer;
