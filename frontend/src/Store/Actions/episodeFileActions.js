import { createAction } from 'redux-actions';
import * as types from './actionTypes';
import episodeFileActionHandlers from './episodeFileActionHandlers';

export const fetchEpisodeFiles = episodeFileActionHandlers[types.FETCH_EPISODE_FILES];
export const deleteEpisodeFile = episodeFileActionHandlers[types.DELETE_EPISODE_FILE];
export const deleteEpisodeFiles = episodeFileActionHandlers[types.DELETE_EPISODE_FILES];
export const updateEpisodeFiles = episodeFileActionHandlers[types.UPDATE_EPISODE_FILES];
export const clearEpisodeFiles = createAction(types.CLEAR_EPISODE_FILES);
