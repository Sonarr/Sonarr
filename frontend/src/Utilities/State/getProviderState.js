import _ from 'lodash';
import getSectionState from 'Utilities/State/getSectionState';

function getProviderState(payload, getState, section) {
  const id = payload.id;

  const state = getSectionState(getState(), section, true);
  const pendingChanges = Object.assign({}, state.pendingChanges);
  const pendingFields = state.pendingChanges.fields || {};
  delete pendingChanges.fields;

  const item = id ? _.find(state.items, { id }) : state.selectedSchema || state.schema || {};

  if (item.fields) {
    pendingChanges.fields = _.reduce(item.fields, (result, field) => {
      const value = pendingFields.hasOwnProperty(field.name) ?
        pendingFields[field.name] :
        field.value;

      result.push({
        ...field,
        value
      });

      return result;
    }, []);
  }

  return Object.assign({}, item, pendingChanges);
}

export default getProviderState;
