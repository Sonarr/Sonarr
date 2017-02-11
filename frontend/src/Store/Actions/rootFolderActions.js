import * as types from './actionTypes';
import rootFolderActionHandlers from './rootFolderActionHandlers';

export const fetchRootFolders = rootFolderActionHandlers[types.FETCH_ROOT_FOLDERS];
export const addRootFolder = rootFolderActionHandlers[types.ADD_ROOT_FOLDER];
export const deleteRootFolder = rootFolderActionHandlers[types.DELETE_ROOT_FOLDER];
