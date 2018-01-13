import _ from 'lodash';
import getSectionState from 'Utilities/State/getSectionState';
import updateSectionState from 'Utilities/State/updateSectionState';

const whitelistedProperties = [
  'pageSize',
  'columns',
  'tableOptions'
];

function createSetTableOptionReducer(section) {
  return (state, { payload }) => {
    const newState = Object.assign(
      getSectionState(state, section),
      _.pick(payload, whitelistedProperties));

    return updateSectionState(state, section, newState);
  };
}

export default createSetTableOptionReducer;
