import { createAction } from 'redux-actions';
import * as types from './actionTypes';
import historyActionHandlers from './historyActionHandlers';

export const fetchHistory = historyActionHandlers[types.FETCH_HISTORY];
export const gotoHistoryFirstPage = historyActionHandlers[types.GOTO_FIRST_HISTORY_PAGE];
export const gotoHistoryPreviousPage = historyActionHandlers[types.GOTO_PREVIOUS_HISTORY_PAGE];
export const gotoHistoryNextPage = historyActionHandlers[types.GOTO_NEXT_HISTORY_PAGE];
export const gotoHistoryLastPage = historyActionHandlers[types.GOTO_LAST_HISTORY_PAGE];
export const gotoHistoryPage = historyActionHandlers[types.GOTO_HISTORY_PAGE];
export const setHistorySort = historyActionHandlers[types.SET_HISTORY_SORT];
export const setHistoryFilter = historyActionHandlers[types.SET_HISTORY_FILTER];
export const setHistoryTableOption = createAction(types.SET_HISTORY_TABLE_OPTION);
export const clearHistory = createAction(types.CLEAR_HISTORY);

export const markAsFailed = historyActionHandlers[types.MARK_AS_FAILED];
