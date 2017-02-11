import { createAction } from 'redux-actions';
import * as types from './actionTypes';
import organizePreviewActionHandlers from './organizePreviewActionHandlers';

export const fetchOrganizePreview = organizePreviewActionHandlers[types.FETCH_ORGANIZE_PREVIEW];
export const clearOrganizePreview = createAction(types.CLEAR_ORGANIZE_PREVIEW);
