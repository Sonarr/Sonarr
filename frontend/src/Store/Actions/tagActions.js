import * as types from './actionTypes';
import tagActionHandlers from './tagActionHandlers';

export const fetchTags = tagActionHandlers[types.FETCH_TAGS];
export const addTag = tagActionHandlers[types.ADD_TAG];
