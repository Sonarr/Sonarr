import _ from 'lodash';
import getSectionState from 'Utilities/State/getSectionState';
import updateSectionState from 'Utilities/State/updateSectionState';

function createUpdateServerSideCollectionReducer(section) {
  return (state, { payload }) => {
    if (section === payload.section) {
      const data = payload.data;
      const newState = getSectionState(state, section);

      const serverState = _.omit(data, ['records']);
      const calculatedState = {
        totalPages: Math.max(Math.ceil(data.totalRecords / data.pageSize), 1),
        items: data.records
      };

      return updateSectionState(state, section, Object.assign(newState, serverState, calculatedState));
    }

    return state;
  };
}

export default createUpdateServerSideCollectionReducer;
