import customFilterHandlers from 'Utilities/customFilterHandlers';
import getSectionState from 'Utilities/State/getSectionState';
import updateSectionState from 'Utilities/State/updateSectionState';
import generateUUIDv4 from 'Utilities/String/generateUUIDv4';

function createRemoveCustomFilterReducer(section) {
  return (state, { payload }) => {
    const newState = getSectionState(state, section);
    const index = newState.customFilters.findIndex((c) => c.key === payload.key);

    newState.customFilters = [...newState.customFilters];
    newState.customFilters.splice(index, 1);

    // Reset the selected filter to the first filter if the selected filter
    // is being deleted.
    // TODO: Server side collections need to have their collections refetched

    if (newState.selectedFilterKey === payload.key) {
      newState.selectedFilterKey = newState.filters[0].key;
    }

    return updateSectionState(state, section, newState);
  };
}

function createSaveCustomFilterReducer(section) {
  return (state, { payload }) => {
    const newState = getSectionState(state, section);

    const {
      label,
      filters
    } = payload;

    let key = payload.key;

    newState.customFilters = [...newState.customFilters];

    if (key) {
      const index = newState.customFilters.findIndex((c) => c.key === key);

      newState.customFilters.splice(index, 1, { key, label, filters });
    } else {
      key = generateUUIDv4();

      newState.customFilters.push({
        key,
        label,
        filters
      });
    }

    // TODO: Server side collections need to have their collections refetched
    newState.selectedFilterKey = key;

    return updateSectionState(state, section, newState);
  };
}

export default function createCustomFilterReducers(section, handlers) {
  return {
    [handlers[customFilterHandlers.REMOVE]]: createRemoveCustomFilterReducer(section),
    [handlers[customFilterHandlers.SAVE]]: createSaveCustomFilterReducer(section)
  };
}
