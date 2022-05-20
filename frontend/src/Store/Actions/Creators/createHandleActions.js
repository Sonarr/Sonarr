import _ from 'lodash';
import { handleActions } from 'redux-actions';
import {
  CLEAR_PENDING_CHANGES,
  REMOVE_ITEM,
  SET,
  UPDATE,
  UPDATE_ITEM,
  UPDATE_SERVER_SIDE_COLLECTION } from 'Store/Actions/baseActions';
import getSectionState from 'Utilities/State/getSectionState';
import updateSectionState from 'Utilities/State/updateSectionState';

const omittedProperties = [
  'section',
  'id'
];

function createItemMap(data) {
  return data.reduce((acc, d, index) => {
    acc[d.id] = index;
    return acc;
  }, {});
}

export default function createHandleActions(handlers, defaultState, section) {
  return handleActions({

    [SET]: function(state, { payload }) {
      const payloadSection = payload.section;
      const [baseSection] = payloadSection.split('.');

      if (section === baseSection) {
        const newState = Object.assign(getSectionState(state, payloadSection),
          _.omit(payload, omittedProperties));

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
          newState.itemMap = createItemMap(payload.data);
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

        // Client side collections that are created by adding items to an
        // existing array may not have an itemMap, the array is probably empty,
        // but on the offchance it's not create a new item map based on the
        // items in the array.
        const itemMap = newState.itemMap ?? createItemMap(items);
        const index = payload.id in itemMap ? itemMap[payload.id] : -1;

        newState.items = [...items];

        // TODO: Move adding to it's own reducer
        if (index >= 0) {
          const item = items[index];
          const newItem = { ...item, ...otherProps };

          // if the item to update is equal to existing, then don't actually update
          // to prevent costly reselections
          if (_.isEqual(item, newItem)) {
            return state;
          }

          newState.items.splice(index, 1, newItem);
        } else if (!updateOnly) {
          const newIndex = newState.items.push({ ...otherProps }) - 1;

          newState.itemMap = { ...itemMap };
          newState.itemMap[payload.id] = newIndex;
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

        newState.itemMap = createItemMap(newState.items);

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
          items: data.records,
          itemMap: createItemMap(data.records)
        };

        return updateSectionState(state, payloadSection, Object.assign(newState, serverState, calculatedState));
      }

      return state;
    },

    ...handlers

  }, defaultState);
}
