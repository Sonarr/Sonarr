import _ from 'lodash';
import getSectionState from 'Utilities/State/getSectionState';
import updateSectionState from 'Utilities/State/updateSectionState';

function createUpdateItemReducer(section, idProp = 'id') {
  return (state, { payload }) => {
    const {
      section: payloadSection,
      updateOnly = false,
      ...otherProps
    } = payload;

    if (section === payloadSection) {
      const newState = getSectionState(state, section);
      const items = newState.items;
      const index = _.findIndex(items, { [idProp]: payload[idProp] });

      newState.items = [...items];

      // TODO: Move adding to it's own reducer
      if (index >= 0) {
        const item = items[index];

        newState.items.splice(index, 1, { ...item, ...otherProps });
      } else if (!updateOnly) {
        newState.items.push({ ...otherProps });
      }

      return updateSectionState(state, section, newState);
    }

    return state;
  };
}

export default createUpdateItemReducer;
