import { handleActions } from 'redux-actions';
import * as types from 'Store/Actions/actionTypes';
import { filterTypes, sortDirections } from 'Helpers/Props';
import createSetReducer from './Creators/createSetReducer';
import createSetClientSideCollectionSortReducer from './Creators/createSetClientSideCollectionSortReducer';
import createSetClientSideCollectionFilterReducer from './Creators/createSetClientSideCollectionFilterReducer';

export const defaultState = {
  isSaving: false,
  saveError: null,
  isDeleting: false,
  deleteError: null,
  sortKey: 'sortTitle',
  sortDirection: sortDirections.ASCENDING,
  secondarySortKey: 'sortTitle',
  secondarySortDirection: sortDirections.ASCENDING,
  filterKey: null,
  filterValue: null,
  filterType: filterTypes.EQUAL
};

export const persistState = [
  'seriesEditor.sortKey',
  'seriesEditor.sortDirection',
  'seriesEditor.filterKey',
  'seriesEditor.filterValue',
  'seriesEditor.filterType'
];

const reducerSection = 'seriesEditor';

const seriesEditorReducers = handleActions({

  [types.SET]: createSetReducer(reducerSection),

  [types.SET_SERIES_EDITOR_SORT]: createSetClientSideCollectionSortReducer(reducerSection),
  [types.SET_SERIES_EDITOR_FILTER]: createSetClientSideCollectionFilterReducer(reducerSection)

}, defaultState);

export default seriesEditorReducers;
