import { handleActions } from 'redux-actions';
import * as types from 'Store/Actions/actionTypes';
import createSetReducer from './Creators/createSetReducer';
import createUpdateItemReducer from './Creators/createUpdateItemReducer';
import createRemoveItemReducer from './Creators/createRemoveItemReducer';

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
  isSidebarVisible: !getDimensions(window.innerWidth, window.innerHeight).isSmallScreen
};

const appReducers = handleActions({

  [types.SAVE_DIMENSIONS]: function(state, { payload }) {
    const {
      width,
      height
    } = payload;

    const dimensions = getDimensions(width, height);

    return Object.assign({}, state, { dimensions });
  },

  [types.SHOW_MESSAGE]: createUpdateItemReducer('messages'),
  [types.HIDE_MESSAGE]: createRemoveItemReducer('messages'),

  [types.SET_APP_VALUE]: createSetReducer('app'),
  [types.SET_VERSION]: function(state, { payload }) {
    const version = payload.version;

    const newState = {
      version
    };

    if (state.version !== version) {
      newState.isUpdated = true;
    }

    return Object.assign({}, state, newState);
  },

  [types.SET_IS_SIDEBAR_VISIBLE]: function(state, { payload }) {
    const newState = {
      isSidebarVisible: payload.isSidebarVisible
    };

    return Object.assign({}, state, newState);
  }

}, defaultState);

export default appReducers;
