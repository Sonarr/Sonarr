import _ from 'lodash';
import $ from 'jquery';
import { createAction } from 'redux-actions';
import getMonitoringOptions from 'Utilities/Series/getMonitoringOptions';
import { sortDirections } from 'Helpers/Props';
import { createThunk, handleThunks } from 'Store/thunks';
import createSetClientSideCollectionSortReducer from './Creators/Reducers/createSetClientSideCollectionSortReducer';
import createSetClientSideCollectionFilterReducer from './Creators/Reducers/createSetClientSideCollectionFilterReducer';
import createHandleActions from './Creators/createHandleActions';
import { set } from './baseActions';
import { fetchSeries, filters, filterPredicates } from './seriesActions';

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
  customFilters: []
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

    let monitoringOptions = null;
    const series = [];
    const allSeries = getState().series.items;

    seriesIds.forEach((id) => {
      const s = _.find(allSeries, { id });
      const seriesToUpdate = { id };

      if (payload.hasOwnProperty('monitored')) {
        seriesToUpdate.monitored = monitored;
      }

      if (monitor) {
        const {
          seasons,
          options: seriesMonitoringOptions
        } = getMonitoringOptions(_.cloneDeep(s.seasons), monitor);

        if (!monitoringOptions) {
          monitoringOptions = seriesMonitoringOptions;
        }

        seriesToUpdate.seasons = seasons;
      }

      series.push(seriesToUpdate);
    });

    dispatch(set({
      section,
      isSaving: true
    }));

    const promise = $.ajax({
      url: '/seasonPass',
      method: 'POST',
      data: JSON.stringify({
        series,
        monitoringOptions
      }),
      dataType: 'json'
    });

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

