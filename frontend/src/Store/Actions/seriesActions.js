import _ from 'lodash';
import $ from 'jquery';
import { createAction } from 'redux-actions';
import { batchActions } from 'redux-batched-actions';
import { filterTypes, sortDirections } from 'Helpers/Props';
import { createThunk, handleThunks } from 'Store/thunks';
import createSetSettingValueReducer from './Creators/Reducers/createSetSettingValueReducer';
import createFetchHandler from './Creators/createFetchHandler';
import createSaveProviderHandler from './Creators/createSaveProviderHandler';
import createRemoveItemHandler from './Creators/createRemoveItemHandler';
import createHandleActions from './Creators/createHandleActions';
import { updateItem } from './baseActions';

//
// Variables

export const section = 'series';

//
// State

export const defaultState = {
  isFetching: false,
  isPopulated: false,
  error: null,
  isSaving: false,
  saveError: null,
  items: [],
  sortKey: 'sortTitle',
  sortDirection: sortDirections.ASCENDING,
  filters: [
    {
      key: 'all',
      label: 'All',
      filters: []
    },
    {
      key: 'monitored',
      label: 'Monitored Only',
      filters: [
        {
          key: 'monitored',
          value: true,
          type: filterTypes.EQUAL
        }
      ]
    },
    {
      key: 'unmonitored',
      label: 'Unmonitored Only',
      filters: [
        {
          key: 'monitored',
          value: false,
          type: filterTypes.EQUAL
        }
      ]
    },
    {
      key: 'continuing',
      label: 'Continuing Only',
      filters: [
        {
          key: 'status',
          value: 'continuing',
          type: filterTypes.EQUAL
        }
      ]
    },
    {
      key: 'ended',
      label: 'Ended Only',
      filters: [
        {
          key: 'status',
          value: 'ended',
          type: filterTypes.EQUAL
        }
      ]
    },
    {
      key: 'missing',
      label: 'Missing Episodes',
      filters: [
        {
          key: 'missing',
          value: true,
          type: filterTypes.EQUAL
        }
      ]
    }
  ],

  filterPredicates: {
    missing: function(item) {
      return item.statistics.episodeCount - item.statistics.episodeFileCount > 0;
    }
  },

  pendingChanges: {}
};

//
// Actions Types

export const FETCH_SERIES = 'series/fetchSeries';
export const SET_SERIES_VALUE = 'series/setSeriesValue';
export const SAVE_SERIES = 'series/saveSeries';
export const DELETE_SERIES = 'series/deleteSeries';

export const TOGGLE_SERIES_MONITORED = 'series/toggleSeriesMonitored';
export const TOGGLE_SEASON_MONITORED = 'series/toggleSeasonMonitored';

//
// Action Creators

export const fetchSeries = createThunk(FETCH_SERIES);
export const saveSeries = createThunk(SAVE_SERIES, (payload) => {
  const newPayload = {
    ...payload
  };

  if (payload.moveFiles) {
    newPayload.queryParams = {
      moveFiles: true
    };
  }

  delete newPayload.moveFiles;

  return newPayload;
});

export const deleteSeries = createThunk(DELETE_SERIES, (payload) => {
  return {
    ...payload,
    queryParams: {
      deleteFiles: payload.deleteFiles
    }
  };
});

export const toggleSeriesMonitored = createThunk(TOGGLE_SERIES_MONITORED);
export const toggleSeasonMonitored = createThunk(TOGGLE_SEASON_MONITORED);

export const setSeriesValue = createAction(SET_SERIES_VALUE, (payload) => {
  return {
    section: 'series',
    ...payload
  };
});

//
// Helpers

function getSaveAjaxOptions({ ajaxOptions, payload }) {
  if (payload.moveFolder) {
    ajaxOptions.url = `${ajaxOptions.url}?moveFolder=true`;
  }

  return ajaxOptions;
}

//
// Action Handlers

export const actionHandlers = handleThunks({

  [FETCH_SERIES]: createFetchHandler(section, '/series'),
  [SAVE_SERIES]: createSaveProviderHandler(section, '/series', { getAjaxOptions: getSaveAjaxOptions }),
  [DELETE_SERIES]: createRemoveItemHandler(section, '/series'),

  [TOGGLE_SERIES_MONITORED]: (getState, payload, dispatch) => {
    const {
      seriesId: id,
      monitored
    } = payload;

    const series = _.find(getState().series.items, { id });

    dispatch(updateItem({
      id,
      section,
      isSaving: true
    }));

    const promise = $.ajax({
      url: `/series/${id}`,
      method: 'PUT',
      data: JSON.stringify({
        ...series,
        monitored
      }),
      dataType: 'json'
    });

    promise.done((data) => {
      dispatch(updateItem({
        id,
        section,
        isSaving: false,
        monitored
      }));
    });

    promise.fail((xhr) => {
      dispatch(updateItem({
        id,
        section,
        isSaving: false
      }));
    });
  },

  [TOGGLE_SEASON_MONITORED]: function(getState, payload, dispatch) {
    const {
      seriesId: id,
      seasonNumber,
      monitored
    } = payload;

    const series = _.find(getState().series.items, { id });
    const seasons = _.cloneDeep(series.seasons);
    const season = _.find(seasons, { seasonNumber });

    season.isSaving = true;

    dispatch(updateItem({
      id,
      section,
      seasons
    }));

    season.monitored = monitored;

    const promise = $.ajax({
      url: `/series/${id}`,
      method: 'PUT',
      data: JSON.stringify({
        ...series,
        seasons
      }),
      dataType: 'json'
    });

    promise.done((data) => {
      const episodes = _.filter(getState().episodes.items, { seriesId: id, seasonNumber });

      dispatch(batchActions([
        updateItem({
          id,
          section,
          ...data
        }),

        ...episodes.map((episode) => {
          return updateItem({
            id: episode.id,
            section: 'episodes',
            monitored
          });
        })
      ]));
    });

    promise.fail((xhr) => {
      dispatch(updateItem({
        id,
        section,
        seasons: series.seasons
      }));
    });
  }

});

//
// Reducers

export const reducers = createHandleActions({

  [SET_SERIES_VALUE]: createSetSettingValueReducer(section)

}, defaultState, section);
