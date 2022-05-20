import _ from 'lodash';
import moment from 'moment';
import { createAction } from 'redux-actions';
import { batchActions } from 'redux-batched-actions';
import * as calendarViews from 'Calendar/calendarViews';
import * as commandNames from 'Commands/commandNames';
import { filterTypes } from 'Helpers/Props';
import { createThunk, handleThunks } from 'Store/thunks';
import createAjaxRequest from 'Utilities/createAjaxRequest';
import { set, update } from './baseActions';
import { executeCommandHelper } from './commandActions';
import createHandleActions from './Creators/createHandleActions';
import createClearReducer from './Creators/Reducers/createClearReducer';

//
// Variables

export const section = 'calendar';

const viewRanges = {
  [calendarViews.DAY]: 'day',
  [calendarViews.WEEK]: 'week',
  [calendarViews.MONTH]: 'month',
  [calendarViews.FORECAST]: 'day'
};

//
// State

export const defaultState = {
  isFetching: false,
  isPopulated: false,
  start: null,
  end: null,
  dates: [],
  dayCount: 7,
  view: window.innerWidth > 768 ? 'week' : 'day',
  error: null,
  items: [],
  searchMissingCommandId: null,

  options: {
    collapseMultipleEpisodes: false,
    showEpisodeInformation: true,
    showFinaleIcon: false,
    showSpecialIcon: false,
    showCutoffUnmetIcon: false,
    fullColorEvents: false
  },

  selectedFilterKey: 'monitored',

  filters: [
    {
      key: 'all',
      label: 'All',
      filters: [
        {
          key: 'monitored',
          value: false,
          type: filterTypes.EQUAL
        }
      ]
    },
    {
      key: 'monitored',
      label: 'Monitored Only',
      filters: [
        {
          key: 'monitored',
          value: true,
          type: filterTypes.EQUAL
        }
      ]
    }

  ]
};

export const persistState = [
  'calendar.view',
  'calendar.selectedFilterKey',
  'calendar.options'
];

//
// Actions Types

export const FETCH_CALENDAR = 'calendar/fetchCalendar';
export const SET_CALENDAR_DAYS_COUNT = 'calendar/setCalendarDaysCount';
export const SET_CALENDAR_FILTER = 'calendar/setCalendarFilter';
export const SET_CALENDAR_VIEW = 'calendar/setCalendarView';
export const GOTO_CALENDAR_TODAY = 'calendar/gotoCalendarToday';
export const GOTO_CALENDAR_NEXT_RANGE = 'calendar/gotoCalendarNextRange';
export const CLEAR_CALENDAR = 'calendar/clearCalendar';
export const SET_CALENDAR_OPTION = 'calendar/setCalendarOption';
export const SEARCH_MISSING = 'calendar/searchMissing';
export const GOTO_CALENDAR_PREVIOUS_RANGE = 'calendar/gotoCalendarPreviousRange';

//
// Helpers

function getDays(start, end) {
  const startTime = moment(start);
  const endTime = moment(end);
  const difference = endTime.diff(startTime, 'days');

  // Difference is one less than the number of days we need to account for.
  return _.times(difference + 1, (i) => {
    return startTime.clone().add(i, 'days').toISOString();
  });
}

function getDates(time, view, firstDayOfWeek, dayCount) {
  const weekName = firstDayOfWeek === 0 ? 'week' : 'isoWeek';

  let start = time.clone().startOf('day');
  let end = time.clone().endOf('day');

  if (view === calendarViews.WEEK) {
    start = time.clone().startOf(weekName);
    end = time.clone().endOf(weekName);
  }

  if (view === calendarViews.FORECAST) {
    start = time.clone().subtract(1, 'day').startOf('day');
    end = time.clone().add(dayCount - 2, 'days').endOf('day');
  }

  if (view === calendarViews.MONTH) {
    start = time.clone().startOf('month').startOf(weekName);
    end = time.clone().endOf('month').endOf(weekName);
  }

  if (view === calendarViews.AGENDA) {
    start = time.clone().subtract(1, 'day').startOf('day');
    end = time.clone().add(1, 'month').endOf('day');
  }

  return {
    start: start.toISOString(),
    end: end.toISOString(),
    time: time.toISOString(),
    dates: getDays(start, end)
  };
}

function getPopulatableRange(startDate, endDate, view) {
  switch (view) {
    case calendarViews.DAY:
      return {
        start: moment(startDate).subtract(1, 'day').toISOString(),
        end: moment(endDate).add(1, 'day').toISOString()
      };
    case calendarViews.WEEK:
    case calendarViews.FORECAST:
      return {
        start: moment(startDate).subtract(1, 'week').toISOString(),
        end: moment(endDate).add(1, 'week').toISOString()
      };
    default:
      return {
        start: startDate,
        end: endDate
      };
  }
}

function isRangePopulated(start, end, state) {
  const {
    start: currentStart,
    end: currentEnd,
    view: currentView
  } = state;

  if (!currentStart || !currentEnd) {
    return false;
  }

  const {
    start: currentPopulatedStart,
    end: currentPopulatedEnd
  } = getPopulatableRange(currentStart, currentEnd, currentView);

  if (
    moment(start).isAfter(currentPopulatedStart) &&
    moment(start).isBefore(currentPopulatedEnd)
  ) {
    return true;
  }

  return false;
}

//
// Action Creators

export const fetchCalendar = createThunk(FETCH_CALENDAR);
export const setCalendarDaysCount = createThunk(SET_CALENDAR_DAYS_COUNT);
export const setCalendarFilter = createThunk(SET_CALENDAR_FILTER);
export const setCalendarView = createThunk(SET_CALENDAR_VIEW);
export const gotoCalendarToday = createThunk(GOTO_CALENDAR_TODAY);
export const gotoCalendarPreviousRange = createThunk(GOTO_CALENDAR_PREVIOUS_RANGE);
export const gotoCalendarNextRange = createThunk(GOTO_CALENDAR_NEXT_RANGE);
export const clearCalendar = createAction(CLEAR_CALENDAR);
export const setCalendarOption = createAction(SET_CALENDAR_OPTION);
export const searchMissing = createThunk(SEARCH_MISSING);

//
// Action Handlers

export const actionHandlers = handleThunks({
  [FETCH_CALENDAR]: function(getState, payload, dispatch) {
    const state = getState();
    const calendar = state.calendar;
    const unmonitored = calendar.selectedFilterKey === 'all';

    const {
      time = calendar.time,
      view = calendar.view
    } = payload;

    const dayCount = state.calendar.dayCount;
    const dates = getDates(moment(time), view, state.settings.ui.item.firstDayOfWeek, dayCount);
    const { start, end } = getPopulatableRange(dates.start, dates.end, view);
    const isPrePopulated = isRangePopulated(start, end, state.calendar);

    const basesAttrs = {
      section,
      isFetching: true
    };

    const attrs = isPrePopulated ?
      {
        view,
        ...basesAttrs,
        ...dates
      } :
      basesAttrs;

    dispatch(set(attrs));

    const promise = createAjaxRequest({
      url: '/calendar',
      data: {
        unmonitored,
        start,
        end
      }
    }).request;

    promise.done((data) => {
      dispatch(batchActions([
        update({ section, data }),

        set({
          section,
          view,
          ...dates,
          isFetching: false,
          isPopulated: true,
          error: null
        })
      ]));
    });

    promise.fail((xhr) => {
      dispatch(set({
        section,
        isFetching: false,
        isPopulated: false,
        error: xhr
      }));
    });
  },

  [SET_CALENDAR_DAYS_COUNT]: function(getState, payload, dispatch) {
    if (payload.dayCount === getState().calendar.dayCount) {
      return;
    }

    dispatch(set({
      section,
      dayCount: payload.dayCount
    }));

    const state = getState();
    const { time, view } = state.calendar;

    dispatch(fetchCalendar({ time, view }));
  },

  [SET_CALENDAR_FILTER]: function(getState, payload, dispatch) {
    dispatch(set({
      section,
      selectedFilterKey: payload.selectedFilterKey
    }));

    const state = getState();
    const { time, view } = state.calendar;

    dispatch(fetchCalendar({ time, view }));
  },

  [SET_CALENDAR_VIEW]: function(getState, payload, dispatch) {
    const state = getState();
    const view = payload.view;
    const time = view === calendarViews.FORECAST || calendarViews.AGENDA ?
      moment() :
      state.calendar.time;

    dispatch(fetchCalendar({ time, view }));
  },

  [GOTO_CALENDAR_TODAY]: function(getState, payload, dispatch) {
    const state = getState();
    const view = state.calendar.view;
    const time = moment();

    dispatch(fetchCalendar({ time, view }));
  },

  [GOTO_CALENDAR_PREVIOUS_RANGE]: function(getState, payload, dispatch) {
    const state = getState();

    const {
      view,
      dayCount
    } = state.calendar;

    const amount = view === calendarViews.FORECAST ? dayCount : 1;
    const time = moment(state.calendar.time).subtract(amount, viewRanges[view]);

    dispatch(fetchCalendar({ time, view }));
  },

  [GOTO_CALENDAR_NEXT_RANGE]: function(getState, payload, dispatch) {
    const state = getState();

    const {
      view,
      dayCount
    } = state.calendar;

    const amount = view === calendarViews.FORECAST ? dayCount : 1;
    const time = moment(state.calendar.time).add(amount, viewRanges[view]);

    dispatch(fetchCalendar({ time, view }));
  },

  [SEARCH_MISSING]: function(getState, payload, dispatch) {
    const { episodeIds } = payload;

    const commandPayload = {
      name: commandNames.EPISODE_SEARCH,
      episodeIds
    };

    executeCommandHelper(commandPayload, dispatch).then((data) => {
      dispatch(set({
        section,
        searchMissingCommandId: data.id
      }));
    });
  }
});

//
// Reducers

export const reducers = createHandleActions({

  [CLEAR_CALENDAR]: createClearReducer(section, {
    isFetching: false,
    isPopulated: false,
    error: null,
    items: []
  }),

  [SET_CALENDAR_OPTION]: function(state, { payload }) {
    const options = state.options;

    return {
      ...state,
      options: {
        ...options,
        ...payload
      }
    };
  }

}, defaultState, section);
