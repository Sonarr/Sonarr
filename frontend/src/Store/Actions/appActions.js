import _ from 'lodash';
import { createAction } from 'redux-actions';
import getSectionState from 'Utilities/State/getSectionState';
import updateSectionState from 'Utilities/State/updateSectionState';
import createHandleActions from './Creators/createHandleActions';

function getDimensions(width, height) {
  const dimensions = {
    width,
    height,
    isExtraSmallScreen: width <= 480,
    isSmallScreen: width <= 768,
    isMediumScreen: width <= 992,
    isLargeScreen: width <= 1200
  };

  return dimensions;
}

//
// Variables

export const section = 'app';
const messagesSection = 'app.messages';

//
// State

export const defaultState = {
  dimensions: getDimensions(window.innerWidth, window.innerHeight),
  messages: {
    items: []
  },
  version: window.Sonarr.version,
  isUpdated: false,
  isConnected: true,
  isReconnecting: false,
  isDisconnected: false,
  isRestarting: false,
  isSidebarVisible: !getDimensions(window.innerWidth, window.innerHeight).isSmallScreen
};

//
// Action Types

export const SHOW_MESSAGE = 'app/showMessage';
export const HIDE_MESSAGE = 'app/hideMessage';
export const SAVE_DIMENSIONS = 'app/saveDimensions';
export const SET_VERSION = 'app/setVersion';
export const SET_APP_VALUE = 'app/setAppValue';
export const SET_IS_SIDEBAR_VISIBLE = 'app/setIsSidebarVisible';

//
// Action Creators

export const saveDimensions = createAction(SAVE_DIMENSIONS);
export const setVersion = createAction(SET_VERSION);
export const setIsSidebarVisible = createAction(SET_IS_SIDEBAR_VISIBLE);
export const setAppValue = createAction(SET_APP_VALUE);
export const showMessage = createAction(SHOW_MESSAGE);
export const hideMessage = createAction(HIDE_MESSAGE);

//
// Reducers

export const reducers = createHandleActions({

  [SAVE_DIMENSIONS]: function(state, { payload }) {
    const {
      width,
      height
    } = payload;

    const dimensions = getDimensions(width, height);

    return Object.assign({}, state, { dimensions });
  },

  [SHOW_MESSAGE]: function(state, { payload }) {
    const newState = getSectionState(state, messagesSection);
    const items = newState.items;
    const index = _.findIndex(items, { id: payload.id });

    newState.items = [...items];

    if (index >= 0) {
      const item = items[index];

      newState.items.splice(index, 1, { ...item, ...payload });
    } else {
      newState.items.push({ ...payload });
    }

    return updateSectionState(state, messagesSection, newState);
  },

  [HIDE_MESSAGE]: function(state, { payload }) {
    const newState = getSectionState(state, messagesSection);

    newState.items = [...newState.items];
    _.remove(newState.items, { id: payload.id });

    return updateSectionState(state, messagesSection, newState);
  },

  [SET_APP_VALUE]: function(state, { payload }) {
    const newState = Object.assign(getSectionState(state, section), payload);

    return updateSectionState(state, section, newState);
  },

  [SET_VERSION]: function(state, { payload }) {
    const version = payload.version;

    const newState = {
      version
    };

    if (state.version !== version) {
      newState.isUpdated = true;
    }

    return Object.assign({}, state, newState);
  },

  [SET_IS_SIDEBAR_VISIBLE]: function(state, { payload }) {
    const newState = {
      isSidebarVisible: payload.isSidebarVisible
    };

    return Object.assign({}, state, newState);
  }

}, defaultState, section);

