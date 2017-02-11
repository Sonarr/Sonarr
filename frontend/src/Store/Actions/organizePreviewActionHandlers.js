import createFetchHandler from './Creators/createFetchHandler';
import * as types from './actionTypes';

const organizePreviewActionHandlers = {
  [types.FETCH_ORGANIZE_PREVIEW]: createFetchHandler('organizePreview', '/rename')
};

export default organizePreviewActionHandlers;
