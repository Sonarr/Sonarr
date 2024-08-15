import { createAction } from 'redux-actions';
import createBulkRemoveItemHandler from 'Store/Actions/Creators/createBulkRemoveItemHandler';
import createRemoveItemHandler from 'Store/Actions/Creators/createRemoveItemHandler';
import createSaveProviderHandler from 'Store/Actions/Creators/createSaveProviderHandler';
import createServerSideCollectionHandlers from 'Store/Actions/Creators/createServerSideCollectionHandlers';
import createClearReducer from 'Store/Actions/Creators/Reducers/createClearReducer';
import createSetSettingValueReducer from 'Store/Actions/Creators/Reducers/createSetSettingValueReducer';
import createSetTableOptionReducer from 'Store/Actions/Creators/Reducers/createSetTableOptionReducer';
import { createThunk, handleThunks } from 'Store/thunks';
import serverSideCollectionHandlers from 'Utilities/State/serverSideCollectionHandlers';

//
// Variables

const section = 'settings.importListExclusions';

//
// Actions Types

export const FETCH_IMPORT_LIST_EXCLUSIONS = 'settings/importListExclusions/fetchImportListExclusions';
export const GOTO_IMPORT_LIST_EXCLUSION_PAGE = 'settings/importListExclusions/gotoImportListExclusionPage';
export const SET_IMPORT_LIST_EXCLUSION_SORT = 'settings/importListExclusions/setImportListExclusionSort';
export const SAVE_IMPORT_LIST_EXCLUSION = 'settings/importListExclusions/saveImportListExclusion';
export const DELETE_IMPORT_LIST_EXCLUSION = 'settings/importListExclusions/deleteImportListExclusion';
export const BULK_DELETE_IMPORT_LIST_EXCLUSIONS = 'settings/importListExclusions/bulkDeleteImportListExclusions';
export const CLEAR_IMPORT_LIST_EXCLUSIONS = 'settings/importListExclusions/clearImportListExclusions';

export const SET_IMPORT_LIST_EXCLUSION_TABLE_OPTION = 'settings/importListExclusions/setImportListExclusionTableOption';
export const SET_IMPORT_LIST_EXCLUSION_VALUE = 'settings/importListExclusions/setImportListExclusionValue';

//
// Action Creators

export const fetchImportListExclusions = createThunk(FETCH_IMPORT_LIST_EXCLUSIONS);
export const gotoImportListExclusionPage = createThunk(GOTO_IMPORT_LIST_EXCLUSION_PAGE);
export const setImportListExclusionSort = createThunk(SET_IMPORT_LIST_EXCLUSION_SORT);
export const saveImportListExclusion = createThunk(SAVE_IMPORT_LIST_EXCLUSION);
export const deleteImportListExclusion = createThunk(DELETE_IMPORT_LIST_EXCLUSION);
export const bulkDeleteImportListExclusions = createThunk(BULK_DELETE_IMPORT_LIST_EXCLUSIONS);
export const clearImportListExclusions = createAction(CLEAR_IMPORT_LIST_EXCLUSIONS);

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
    isDeleting: false,
    deleteError: null,
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
        [serverSideCollectionHandlers.EXACT_PAGE]: GOTO_IMPORT_LIST_EXCLUSION_PAGE,
        [serverSideCollectionHandlers.SORT]: SET_IMPORT_LIST_EXCLUSION_SORT
      }
    ),
    [SAVE_IMPORT_LIST_EXCLUSION]: createSaveProviderHandler(section, '/importlistexclusion'),
    [DELETE_IMPORT_LIST_EXCLUSION]: createRemoveItemHandler(section, '/importlistexclusion'),
    [BULK_DELETE_IMPORT_LIST_EXCLUSIONS]: createBulkRemoveItemHandler(section, '/importlistexclusion/bulk')
  }),

  //
  // Reducers

  reducers: {
    [SET_IMPORT_LIST_EXCLUSION_VALUE]: createSetSettingValueReducer(section),
    [SET_IMPORT_LIST_EXCLUSION_TABLE_OPTION]: createSetTableOptionReducer(section),

    [CLEAR_IMPORT_LIST_EXCLUSIONS]: createClearReducer(section, {
      isFetching: false,
      isPopulated: false,
      error: null,
      items: [],
      isDeleting: false,
      deleteError: null,
      pendingChanges: {},
      totalPages: 0,
      totalRecords: 0
    })
  }

};
