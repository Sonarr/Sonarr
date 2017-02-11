import _ from 'lodash';
import { handleActions } from 'redux-actions';
import * as types from 'Store/Actions/actionTypes';
import createSetReducer from './Creators/createSetReducer';
import createUpdateReducer from './Creators/createUpdateReducer';
import createUpdateItemReducer from './Creators/createUpdateItemReducer';

export const defaultState = {
  isFetching: false,
  isPopulated: false,
  error: null,
  items: [],
  handlers: {}
};

const reducerSection = 'commands';

const commandReducers = handleActions({

  [types.SET]: createSetReducer(reducerSection),
  [types.UPDATE]: createUpdateReducer(reducerSection),
  [types.UPDATE_ITEM]: createUpdateItemReducer(reducerSection),

  [types.ADD_COMMAND]: (state, { payload }) => {
    const newState = Object.assign({}, state);
    newState.items = [...state.items, payload];

    return newState;
  },

  [types.REMOVE_COMMAND]: (state, { payload }) => {
    const newState = Object.assign({}, state);
    newState.items = [...state.items];

    const index = _.findIndex(newState.items, { id: payload.id });

    if (index > -1) {
      newState.items.splice(index, 1);
    }

    return newState;
  },

  [types.REGISTER_FINISH_COMMAND_HANDLER]: (state, { payload }) => {
    const newState = Object.assign({}, state);

    newState.handlers[payload.key] = {
      name: payload.name,
      handler: payload.handler
    };

    return newState;
  },

  [types.UNREGISTER_FINISH_COMMAND_HANDLER]: (state, { payload }) => {
    const newState = Object.assign({}, state);
    delete newState.handlers[payload.key];

    return newState;
  }

}, defaultState);

export default commandReducers;
