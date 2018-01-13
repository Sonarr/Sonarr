import _ from 'lodash';
import { handleActions } from 'redux-actions';
import getSectionState from 'Utilities/State/getSectionState';
import updateSectionState from 'Utilities/State/updateSectionState';
import {
  SET,
  UPDATE,
  UPDATE_ITEM,
  UPDATE_SERVER_SIDE_COLLECTION,
  CLEAR_PENDING_CHANGES,
  REMOVE_ITEM
} from 'Store/Actions/baseActions';

const blacklistedProperties = [
  'section',
  'id'
];

export default function createHandleActions(handlers, defaultState, section) {
  return handleActions({

    [SET]: function(state, { payload }) {
      const payloadSection = payload.section;
      const [baseSection] = payloadSection.split('.');

      if (section === baseSection) {
        const newState = Object.assign(getSectionState(state, payloadSection),
          _.omit(payload, blacklistedProperties));

        return updateSectionState(state, payloadSection, newState);
      }

      return state;
    },

    [UPDATE]: function(state, { payload }) {
      const payloadSection = payload.section;
      const [baseSection] = payloadSection.split('.');

      if (section === baseSection) {
        const newState = getSectionState(state, payloadSection);

        if (_.isArray(payload.data)) {
          newState.items = payload.data;
        } else {
          newState.item = payload.data;
        }

        return updateSectionState(state, payloadSection, newState);
      }

      return state;
    },

    [UPDATE_ITEM]: function(state, { payload }) {
      const {
        section: payloadSection,
        updateOnly = false,
        ...otherProps
      } = payload;

      const [baseSection] = payloadSection.split('.');

      if (section === baseSection) {
        const newState = getSectionState(state, payloadSection);
        const items = newState.items;
        const index = _.findIndex(items, { id: payload.id });

        newState.items = [...items];

        // TODO: Move adding to it's own reducer
        if (index >= 0) {
          const item = items[index];

          newState.items.splice(index, 1, { ...item, ...otherProps });
        } else if (!updateOnly) {
          newState.items.push({ ...otherProps });
        }

        return updateSectionState(state, payloadSection, newState);
      }

      return state;
    },

    [CLEAR_PENDING_CHANGES]: function(state, { payload }) {
      const payloadSection = payload.section;
      const [baseSection] = payloadSection.split('.');

      if (section === baseSection) {
        const newState = getSectionState(state, payloadSection);
        newState.pendingChanges = {};

        if (newState.hasOwnProperty('saveError')) {
          newState.saveError = null;
        }

        return updateSectionState(state, payloadSection, newState);
      }

      return state;
    },

    [REMOVE_ITEM]: function(state, { payload }) {
      const payloadSection = payload.section;
      const [baseSection] = payloadSection.split('.');

      if (section === baseSection) {
        const newState = getSectionState(state, payloadSection);

        newState.items = [...newState.items];
        _.remove(newState.items, { id: payload.id });

        return updateSectionState(state, payloadSection, newState);
      }

      return state;
    },

    [UPDATE_SERVER_SIDE_COLLECTION]: function(state, { payload }) {
      const payloadSection = payload.section;
      const [baseSection] = payloadSection.split('.');

      if (section === baseSection) {
        const data = payload.data;
        const newState = getSectionState(state, payloadSection);

        const serverState = _.omit(data, ['records']);
        const calculatedState = {
          totalPages: Math.max(Math.ceil(data.totalRecords / data.pageSize), 1),
          items: data.records
        };

        return updateSectionState(state, payloadSection, Object.assign(newState, serverState, calculatedState));
      }

      return state;
    },

    ...handlers

  }, defaultState);
}
