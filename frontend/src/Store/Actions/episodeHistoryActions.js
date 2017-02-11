import { createAction } from 'redux-actions';
import * as types from './actionTypes';
import episodeHistoryActionHandlers from './episodeHistoryActionHandlers';

export const fetchEpisodeHistory = episodeHistoryActionHandlers[types.FETCH_EPISODE_HISTORY];
export const clearEpisodeHistory = createAction(types.CLEAR_EPISODE_HISTORY);
export const episodeHistoryMarkAsFailed = episodeHistoryActionHandlers[types.EPISODE_HISTORY_MARK_AS_FAILED];
