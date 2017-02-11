import { createAction } from 'redux-actions';
import * as types from './actionTypes';
import queueActionHandlers from './queueActionHandlers';

export const fetchQueueStatus = queueActionHandlers[types.FETCH_QUEUE_STATUS];

export const fetchQueueDetails = queueActionHandlers[types.FETCH_QUEUE_DETAILS];
export const clearQueueDetails = createAction(types.CLEAR_QUEUE_DETAILS);

export const fetchQueue = queueActionHandlers[types.FETCH_QUEUE];
export const gotoQueueFirstPage = queueActionHandlers[types.GOTO_FIRST_QUEUE_PAGE];
export const gotoQueuePreviousPage = queueActionHandlers[types.GOTO_PREVIOUS_QUEUE_PAGE];
export const gotoQueueNextPage = queueActionHandlers[types.GOTO_NEXT_QUEUE_PAGE];
export const gotoQueueLastPage = queueActionHandlers[types.GOTO_LAST_QUEUE_PAGE];
export const gotoQueuePage = queueActionHandlers[types.GOTO_QUEUE_PAGE];
export const setQueueSort = queueActionHandlers[types.SET_QUEUE_SORT];
export const setQueueTableOption = createAction(types.SET_QUEUE_TABLE_OPTION);
export const clearQueue = createAction(types.CLEAR_QUEUE);

export const grabQueueItem = queueActionHandlers[types.GRAB_QUEUE_ITEM];
export const grabQueueItems = queueActionHandlers[types.GRAB_QUEUE_ITEMS];
export const removeQueueItem = queueActionHandlers[types.REMOVE_QUEUE_ITEM];
export const removeQueueItems = queueActionHandlers[types.REMOVE_QUEUE_ITEMS];
