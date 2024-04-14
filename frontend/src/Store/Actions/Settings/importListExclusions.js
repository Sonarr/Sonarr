import { createAction } from 'redux-actions';
import createRemoveItemHandler from 'Store/Actions/Creators/createRemoveItemHandler';
import createSaveProviderHandler from 'Store/Actions/Creators/createSaveProviderHandler';
import createServerSideCollectionHandlers from 'Store/Actions/Creators/createServerSideCollectionHandlers';
import createSetSettingValueReducer from 'Store/Actions/Creators/Reducers/createSetSettingValueReducer';
import createSetTableOptionReducer from 'Store/Actions/Creators/Reducers/createSetTableOptionReducer';
import { createThunk, handleThunks } from 'Store/thunks';
import serverSideCollectionHandlers from 'Utilities/serverSideCollectionHandlers';

//
// Variables

const section = 'settings.importListExclusions';

//
// Actions Types

export const FETCH_IMPORT_LIST_EXCLUSIONS = 'settings/importListExclusions/fetchImportListExclusions';
export const GOTO_FIRST_IMPORT_LIST_EXCLUSION_PAGE = 'settings/importListExclusions/gotoImportListExclusionFirstPage';
export const GOTO_PREVIOUS_IMPORT_LIST_EXCLUSION_PAGE = 'settings/importListExclusions/gotoImportListExclusionPreviousPage';
export const GOTO_NEXT_IMPORT_LIST_EXCLUSION_PAGE = 'settings/importListExclusions/gotoImportListExclusionNextPage';
export const GOTO_LAST_IMPORT_LIST_EXCLUSION_PAGE = 'settings/importListExclusions/gotoImportListExclusionLastPage';
export const GOTO_IMPORT_LIST_EXCLUSION_PAGE = 'settings/importListExclusions/gotoImportListExclusionPage';
export const SET_IMPORT_LIST_EXCLUSION_SORT = 'settings/importListExclusions/setImportListExclusionSort';
export const SET_IMPORT_LIST_EXCLUSION_TABLE_OPTION = 'settings/importListExclusions/setImportListExclusionTableOption';
export const SAVE_IMPORT_LIST_EXCLUSION = 'settings/importListExclusions/saveImportListExclusion';
export const DELETE_IMPORT_LIST_EXCLUSION = 'settings/importListExclusions/deleteImportListExclusion';
export const SET_IMPORT_LIST_EXCLUSION_VALUE = 'settings/importListExclusions/setImportListExclusionValue';

//
// Action Creators

export const fetchImportListExclusions = createThunk(FETCH_IMPORT_LIST_EXCLUSIONS);
export const gotoImportListExclusionFirstPage = createThunk(GOTO_FIRST_IMPORT_LIST_EXCLUSION_PAGE);
export const gotoImportListExclusionPreviousPage = createThunk(GOTO_PREVIOUS_IMPORT_LIST_EXCLUSION_PAGE);
export const gotoImportListExclusionNextPage = createThunk(GOTO_NEXT_IMPORT_LIST_EXCLUSION_PAGE);
export const gotoImportListExclusionLastPage = createThunk(GOTO_LAST_IMPORT_LIST_EXCLUSION_PAGE);
export const gotoImportListExclusionPage = createThunk(GOTO_IMPORT_LIST_EXCLUSION_PAGE);
export const setImportListExclusionSort = createThunk(SET_IMPORT_LIST_EXCLUSION_SORT);
export const saveImportListExclusion = createThunk(SAVE_IMPORT_LIST_EXCLUSION);
export const deleteImportListExclusion = createThunk(DELETE_IMPORT_LIST_EXCLUSION);

export const setImportListExclusionTableOption = createAction(SET_IMPORT_LIST_EXCLUSION_TABLE_OPTION);
export const setImportListExclusionValue = createAction(SET_IMPORT_LIST_EXCLUSION_VALUE, (payload) => {
  return {
    section,
    ...payload
  };
});

//
// Details

export default {

  //
  // State

  defaultState: {
    isFetching: false,
    isPopulated: false,
    error: null,
    pageSize: 20,
    items: [],
    isSaving: false,
    saveError: null,
    pendingChanges: {}
  },

  //
  // Action Handlers

  actionHandlers: handleThunks({
    ...createServerSideCollectionHandlers(
      section,
      '/importlistexclusion/paged',
      fetchImportListExclusions,
      {
        [serverSideCollectionHandlers.FETCH]: FETCH_IMPORT_LIST_EXCLUSIONS,
        [serverSideCollectionHandlers.FIRST_PAGE]: GOTO_FIRST_IMPORT_LIST_EXCLUSION_PAGE,
        [serverSideCollectionHandlers.PREVIOUS_PAGE]: GOTO_PREVIOUS_IMPORT_LIST_EXCLUSION_PAGE,
        [serverSideCollectionHandlers.NEXT_PAGE]: GOTO_NEXT_IMPORT_LIST_EXCLUSION_PAGE,
        [serverSideCollectionHandlers.LAST_PAGE]: GOTO_LAST_IMPORT_LIST_EXCLUSION_PAGE,
        [serverSideCollectionHandlers.EXACT_PAGE]: GOTO_IMPORT_LIST_EXCLUSION_PAGE,
        [serverSideCollectionHandlers.SORT]: SET_IMPORT_LIST_EXCLUSION_SORT
      }
    ),
    [SAVE_IMPORT_LIST_EXCLUSION]: createSaveProviderHandler(section, '/importlistexclusion'),
    [DELETE_IMPORT_LIST_EXCLUSION]: createRemoveItemHandler(section, '/importlistexclusion')
  }),

  //
  // Reducers

  reducers: {
    [SET_IMPORT_LIST_EXCLUSION_VALUE]: createSetSettingValueReducer(section),
    [SET_IMPORT_LIST_EXCLUSION_TABLE_OPTION]: createSetTableOptionReducer(section)
  }

};
