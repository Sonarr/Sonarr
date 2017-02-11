import { createAction } from 'redux-actions';
import * as types from './actionTypes';
import interactiveImportActionHandlers from './interactiveImportActionHandlers';

export const fetchInteractiveImportItems = interactiveImportActionHandlers[types.FETCH_INTERACTIVE_IMPORT_ITEMS];
export const setInteractiveImportSort = createAction(types.SET_INTERACTIVE_IMPORT_SORT);
export const updateInteractiveImportItem = createAction(types.UPDATE_INTERACTIVE_IMPORT_ITEM);
export const clearInteractiveImport = createAction(types.CLEAR_INTERACTIVE_IMPORT);
export const addRecentFolder = createAction(types.ADD_RECENT_FOLDER);
export const removeRecentFolder = createAction(types.REMOVE_RECENT_FOLDER);
export const setInteractiveImportMode = createAction(types.SET_INTERACTIVE_IMPORT_MODE);

export const fetchInteractiveImportEpisodes = interactiveImportActionHandlers[types.FETCH_INTERACTIVE_IMPORT_EPISODES];
export const setInteractiveImportEpisodesSort = createAction(types.SET_INTERACTIVE_IMPORT_EPISODES_SORT);
export const clearInteractiveImportEpisodes = createAction(types.CLEAR_INTERACTIVE_IMPORT_EPISODES);
