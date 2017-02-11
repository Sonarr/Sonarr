import { createAction } from 'redux-actions';
import * as types from './actionTypes';
import blacklistActionHandlers from './blacklistActionHandlers';

export const fetchBlacklist = blacklistActionHandlers[types.FETCH_BLACKLIST];
export const gotoBlacklistFirstPage = blacklistActionHandlers[types.GOTO_FIRST_BLACKLIST_PAGE];
export const gotoBlacklistPreviousPage = blacklistActionHandlers[types.GOTO_PREVIOUS_BLACKLIST_PAGE];
export const gotoBlacklistNextPage = blacklistActionHandlers[types.GOTO_NEXT_BLACKLIST_PAGE];
export const gotoBlacklistLastPage = blacklistActionHandlers[types.GOTO_LAST_BLACKLIST_PAGE];
export const gotoBlacklistPage = blacklistActionHandlers[types.GOTO_BLACKLIST_PAGE];
export const setBlacklistSort = blacklistActionHandlers[types.SET_BLACKLIST_SORT];
export const setBlacklistTableOption = createAction(types.SET_BLACKLIST_TABLE_OPTION);
