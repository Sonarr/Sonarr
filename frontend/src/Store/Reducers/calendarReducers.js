import { handleActions } from 'redux-actions';
import * as types from 'Store/Actions/actionTypes';
import createSetReducer from './Creators/createSetReducer';
import createUpdateReducer from './Creators/createUpdateReducer';
import createUpdateItemReducer from './Creators/createUpdateItemReducer';

export const defaultState = {
  isFetching: false,
  isPopulated: false,
  start: null,
  end: null,
  dates: [],
  dayCount: 7,
  view: window.innerWidth > 768 ? 'week' : 'day',
  unmonitored: false,
  showUpcoming: true,
  error: null,
  items: []
};

export const persistState = [
  'calendar.view',
  'calendar.unmonitored',
  'calendar.showUpcoming'
];

const section = 'calendar';

const calendarReducers = handleActions({

  [types.SET]: createSetReducer(section),
  [types.UPDATE]: createUpdateReducer(section),
  [types.UPDATE_ITEM]: createUpdateItemReducer(section),

  [types.CLEAR_CALENDAR]: (state) => {
    const {
      view,
      unmonitored,
      showUpcoming,
      ...otherDefaultState
    } = defaultState;

    return Object.assign({}, state, otherDefaultState);
  }

}, defaultState);

export default calendarReducers;
