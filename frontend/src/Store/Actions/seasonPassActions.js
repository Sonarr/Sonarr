import { createAction } from 'redux-actions';
import { sortDirections } from 'Helpers/Props';
import { createThunk, handleThunks } from 'Store/thunks';
import createAjaxRequest from 'Utilities/createAjaxRequest';
import { set } from './baseActions';
import createHandleActions from './Creators/createHandleActions';
import createSetClientSideCollectionFilterReducer from './Creators/Reducers/createSetClientSideCollectionFilterReducer';
import createSetClientSideCollectionSortReducer from './Creators/Reducers/createSetClientSideCollectionSortReducer';
import { fetchSeries, filterBuilderProps, filterPredicates, filters } from './seriesActions';

//
// Variables

export const section = 'seasonPass';

//
// State

export const defaultState = {
  isSaving: false,
  saveError: null,
  sortKey: 'sortTitle',
  sortDirection: sortDirections.ASCENDING,
  secondarySortKey: 'sortTitle',
  secondarySortDirection: sortDirections.ASCENDING,
  selectedFilterKey: 'all',
  filters,
  filterPredicates,

  filterBuilderProps
};

export const persistState = [
  'seasonPass.sortKey',
  'seasonPass.sortDirection',
  'seasonPass.selectedFilterKey',
  'seasonPass.customFilters'
];

//
// Actions Types

export const SET_SEASON_PASS_SORT = 'seasonPass/setSeasonPassSort';
export const SET_SEASON_PASS_FILTER = 'seasonPass/setSeasonPassFilter';
export const SAVE_SEASON_PASS = 'seasonPass/saveSeasonPass';

//
// Action Creators

export const setSeasonPassSort = createAction(SET_SEASON_PASS_SORT);
export const setSeasonPassFilter = createAction(SET_SEASON_PASS_FILTER);
export const saveSeasonPass = createThunk(SAVE_SEASON_PASS);

//
// Action Handlers

export const actionHandlers = handleThunks({

  [SAVE_SEASON_PASS]: function(getState, payload, dispatch) {
    const {
      seriesIds,
      monitored,
      monitor
    } = payload;

    const series = [];

    seriesIds.forEach((id) => {
      const seriesToUpdate = { id };

      if (payload.hasOwnProperty('monitored')) {
        seriesToUpdate.monitored = monitored;
      }

      series.push(seriesToUpdate);
    });

    dispatch(set({
      section,
      isSaving: true
    }));

    const promise = createAjaxRequest({
      url: '/seasonPass',
      method: 'POST',
      data: JSON.stringify({
        series,
        monitoringOptions: { monitor }
      }),
      dataType: 'json'
    }).request;

    promise.done((data) => {
      dispatch(fetchSeries());

      dispatch(set({
        section,
        isSaving: false,
        saveError: null
      }));
    });

    promise.fail((xhr) => {
      dispatch(set({
        section,
        isSaving: false,
        saveError: xhr
      }));
    });
  }

});

//
// Reducers

export const reducers = createHandleActions({

  [SET_SEASON_PASS_SORT]: createSetClientSideCollectionSortReducer(section),
  [SET_SEASON_PASS_FILTER]: createSetClientSideCollectionFilterReducer(section)

}, defaultState, section);

