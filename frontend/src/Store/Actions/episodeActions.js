import { createAction } from 'redux-actions';
import * as types from './actionTypes';
import episodeActionHandlers from './episodeActionHandlers';

export const fetchEpisodes = episodeActionHandlers[types.FETCH_EPISODES];
export const setEpisodesSort = createAction(types.SET_EPISODES_SORT);
export const setEpisodesTableOption = createAction(types.SET_EPISODES_TABLE_OPTION);
export const clearEpisodes = createAction(types.CLEAR_EPISODES);
export const toggleEpisodeMonitored = episodeActionHandlers[types.TOGGLE_EPISODE_MONITORED];
export const toggleEpisodesMonitored = episodeActionHandlers[types.TOGGLE_EPISODES_MONITORED];
