import { createAction } from 'redux-actions';
import * as types from './actionTypes';
import calendarActionHandlers from './calendarActionHandlers';

export const fetchCalendar = calendarActionHandlers[types.FETCH_CALENDAR];
export const setCalendarDaysCount = calendarActionHandlers[types.SET_CALENDAR_DAYS_COUNT];
export const setCalendarIncludeUnmonitored = calendarActionHandlers[types.SET_CALENDAR_INCLUDE_UNMONITORED];
export const setCalendarView = calendarActionHandlers[types.SET_CALENDAR_VIEW];
export const gotoCalendarToday = calendarActionHandlers[types.GOTO_CALENDAR_TODAY];
export const gotoCalendarPreviousRange = calendarActionHandlers[types.GOTO_CALENDAR_PREVIOUS_RANGE];
export const gotoCalendarNextRange = calendarActionHandlers[types.GOTO_CALENDAR_NEXT_RANGE];
export const clearCalendar = createAction(types.CLEAR_CALENDAR);
