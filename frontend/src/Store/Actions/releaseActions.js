import { createAction } from 'redux-actions';
import * as types from './actionTypes';
import releaseActionHandlers from './releaseActionHandlers';

export const fetchReleases = releaseActionHandlers[types.FETCH_RELEASES];
export const setReleasesSort = createAction(types.SET_RELEASES_SORT);
export const clearReleases = createAction(types.CLEAR_RELEASES);
export const grabRelease = releaseActionHandlers[types.GRAB_RELEASE];
export const updateRelease = createAction(types.UPDATE_RELEASE);
