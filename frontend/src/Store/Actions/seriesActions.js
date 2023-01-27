import _ from 'lodash';
import { createAction } from 'redux-actions';
import { batchActions } from 'redux-batched-actions';
import { filterBuilderTypes, filterBuilderValueTypes, filterTypePredicates, filterTypes, sortDirections } from 'Helpers/Props';
import { createThunk, handleThunks } from 'Store/thunks';
import sortByName from 'Utilities/Array/sortByName';
import createAjaxRequest from 'Utilities/createAjaxRequest';
import dateFilterPredicate from 'Utilities/Date/dateFilterPredicate';
import { set, updateItem } from './baseActions';
import createFetchHandler from './Creators/createFetchHandler';
import createHandleActions from './Creators/createHandleActions';
import createRemoveItemHandler from './Creators/createRemoveItemHandler';
import createSaveProviderHandler from './Creators/createSaveProviderHandler';
import createSetSettingValueReducer from './Creators/Reducers/createSetSettingValueReducer';
import { fetchEpisodes } from './episodeActions';

//
// Local

const MONITOR_TIMEOUT = 1000;
const seasonsToUpdate = {};
const seasonMonitorToggleTimeouts = {};

//
// Variables

export const section = 'series';

export const filters = [
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
];

export const filterPredicates = {
  episodeProgress: function(item, filterValue, type) {
    const { statistics = {} } = item;

    const {
      episodeCount = 0,
      episodeFileCount
    } = statistics;

    const progress = episodeCount ?
      episodeFileCount / episodeCount * 100 :
      100;

    const predicate = filterTypePredicates[type];

    return predicate(progress, filterValue);
  },

  missing: function(item) {
    const { statistics = {} } = item;

    return statistics.episodeCount - statistics.episodeFileCount > 0;
  },

  nextAiring: function(item, filterValue, type) {
    return dateFilterPredicate(item.nextAiring, filterValue, type);
  },

  previousAiring: function(item, filterValue, type) {
    return dateFilterPredicate(item.previousAiring, filterValue, type);
  },

  added: function(item, filterValue, type) {
    return dateFilterPredicate(item.added, filterValue, type);
  },

  ratings: function(item, filterValue, type) {
    const predicate = filterTypePredicates[type];

    return predicate(item.ratings.value * 10, filterValue);
  },

  originalLanguage: function(item, filterValue, type) {
    const predicate = filterTypePredicates[type];
    const { originalLanguage } = item;

    return predicate(originalLanguage ? originalLanguage.name : '', filterValue);
  },

  releaseGroups: function(item, filterValue, type) {
    const { statistics = {} } = item;

    const {
      releaseGroups = []
    } = statistics;

    const predicate = filterTypePredicates[type];

    return predicate(releaseGroups, filterValue);
  },

  seasonCount: function(item, filterValue, type) {
    const predicate = filterTypePredicates[type];
    const seasonCount = item.statistics ? item.statistics.seasonCount : 0;

    return predicate(seasonCount, filterValue);
  },

  sizeOnDisk: function(item, filterValue, type) {
    const predicate = filterTypePredicates[type];
    const sizeOnDisk = item.statistics && item.statistics.sizeOnDisk ?
      item.statistics.sizeOnDisk :
      0;

    return predicate(sizeOnDisk, filterValue);
  },

  hasMissingSeason: function(item, filterValue, type) {
    const { seasons = [] } = item;

    return seasons.some((season) => {
      const {
        seasonNumber,
        statistics = {}
      } = season;

      const {
        episodeFileCount = 0,
        episodeCount = 0,
        totalEpisodeCount = 0
      } = statistics;

      return (
        seasonNumber > 0 &&
        totalEpisodeCount > 0 &&
        episodeCount === totalEpisodeCount &&
        episodeFileCount === 0
      );
    });
  }
};

export const filterBuilderProps = [
  {
    name: 'monitored',
    label: 'Monitored',
    type: filterBuilderTypes.EXACT,
    valueType: filterBuilderValueTypes.BOOL
  },
  {
    name: 'status',
    label: 'Status',
    type: filterBuilderTypes.EXACT,
    valueType: filterBuilderValueTypes.SERIES_STATUS
  },
  {
    name: 'seriesType',
    label: 'Type',
    type: filterBuilderTypes.EXACT,
    valueType: filterBuilderValueTypes.SERIES_TYPES
  },
  {
    name: 'network',
    label: 'Network',
    type: filterBuilderTypes.STRING,
    optionsSelector: function(items) {
      const tagList = items.reduce((acc, series) => {
        if (series.network) {
          acc.push({
            id: series.network,
            name: series.network
          });
        }

        return acc;
      }, []);

      return tagList.sort(sortByName);
    }
  },
  {
    name: 'qualityProfileId',
    label: 'Quality Profile',
    type: filterBuilderTypes.EXACT,
    valueType: filterBuilderValueTypes.QUALITY_PROFILE
  },
  {
    name: 'nextAiring',
    label: 'Next Airing',
    type: filterBuilderTypes.DATE,
    valueType: filterBuilderValueTypes.DATE
  },
  {
    name: 'previousAiring',
    label: 'Previous Airing',
    type: filterBuilderTypes.DATE,
    valueType: filterBuilderValueTypes.DATE
  },
  {
    name: 'added',
    label: 'Added',
    type: filterBuilderTypes.DATE,
    valueType: filterBuilderValueTypes.DATE
  },
  {
    name: 'seasonCount',
    label: 'Season Count',
    type: filterBuilderTypes.NUMBER
  },
  {
    name: 'episodeProgress',
    label: 'Episode Progress',
    type: filterBuilderTypes.NUMBER
  },
  {
    name: 'path',
    label: 'Path',
    type: filterBuilderTypes.STRING
  },
  {
    name: 'rootFolderPath',
    label: 'Root Folder Path',
    type: filterBuilderTypes.EXACT
  },
  {
    name: 'sizeOnDisk',
    label: 'Size on Disk',
    type: filterBuilderTypes.NUMBER,
    valueType: filterBuilderValueTypes.BYTES
  },
  {
    name: 'genres',
    label: 'Genres',
    type: filterBuilderTypes.ARRAY,
    optionsSelector: function(items) {
      const tagList = items.reduce((acc, series) => {
        series.genres.forEach((genre) => {
          acc.push({
            id: genre,
            name: genre
          });
        });

        return acc;
      }, []);

      return tagList.sort(sortByName);
    }
  },
  {
    name: 'originalLanguage',
    label: 'Original Language',
    type: filterBuilderTypes.EXACT,
    optionsSelector: function(items) {
      const languageList = items.reduce((acc, series) => {
        if (series.originalLanguage) {
          acc.push({
            id: series.originalLanguage.name,
            name: series.originalLanguage.name
          });
        }

        return acc;
      }, []);

      return languageList.sort(sortByName);
    }
  },
  {
    name: 'releaseGroups',
    label: 'Release Groups',
    type: filterBuilderTypes.ARRAY
  },
  {
    name: 'ratings',
    label: 'Rating',
    type: filterBuilderTypes.NUMBER
  },
  {
    name: 'certification',
    label: 'Certification',
    type: filterBuilderTypes.EXACT
  },
  {
    name: 'tags',
    label: 'Tags',
    type: filterBuilderTypes.ARRAY,
    valueType: filterBuilderValueTypes.TAG
  },
  {
    name: 'useSceneNumbering',
    label: 'Scene Numbering',
    type: filterBuilderTypes.EXACT
  },
  {
    name: 'hasMissingSeason',
    label: 'Has Missing Season',
    type: filterBuilderTypes.EXACT
  }
];

export const sortPredicates = {
  status: function(item) {
    let result = 0;

    if (item.monitored) {
      result += 2;
    }

    if (item.status === 'continuing') {
      result++;
    }

    return result;
  },

  sizeOnDisk: function(item) {
    const { statistics = {} } = item;

    return statistics.sizeOnDisk || 0;
  }
};

//
// State

export const defaultState = {
  isFetching: false,
  isPopulated: false,
  error: null,
  isSaving: false,
  saveError: null,
  isDeleting: false,
  deleteError: null,
  items: [],
  sortKey: 'sortTitle',
  sortDirection: sortDirections.ASCENDING,
  pendingChanges: {},
  deleteOptions: {
    addImportListExclusion: false
  }
};

export const persistState = [
  'series.deleteOptions'
];

//
// Actions Types

export const FETCH_SERIES = 'series/fetchSeries';
export const SET_SERIES_VALUE = 'series/setSeriesValue';
export const SAVE_SERIES = 'series/saveSeries';
export const DELETE_SERIES = 'series/deleteSeries';

export const TOGGLE_SERIES_MONITORED = 'series/toggleSeriesMonitored';
export const TOGGLE_SEASON_MONITORED = 'series/toggleSeasonMonitored';
export const UPDATE_SERIES_MONITOR = 'series/updateSeriesMonitor';
export const SAVE_SERIES_EDITOR = 'series/saveSeriesEditor';
export const BULK_DELETE_SERIES = 'series/bulkDeleteSeries';

export const SET_DELETE_OPTION = 'series/setDeleteOption';

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
      deleteFiles: payload.deleteFiles,
      addImportListExclusion: payload.addImportListExclusion
    }
  };
});

export const toggleSeriesMonitored = createThunk(TOGGLE_SERIES_MONITORED);
export const toggleSeasonMonitored = createThunk(TOGGLE_SEASON_MONITORED);
export const updateSeriesMonitor = createThunk(UPDATE_SERIES_MONITOR);
export const saveSeriesEditor = createThunk(SAVE_SERIES_EDITOR);
export const bulkDeleteSeries = createThunk(BULK_DELETE_SERIES);

export const setSeriesValue = createAction(SET_SERIES_VALUE, (payload) => {
  return {
    section,
    ...payload
  };
});

export const setDeleteOption = createAction(SET_DELETE_OPTION);

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

    const promise = createAjaxRequest({
      url: `/series/${id}`,
      method: 'PUT',
      data: JSON.stringify({
        ...series,
        monitored
      }),
      dataType: 'json'
    }).request;

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

    const seasonMonitorToggleTimeout = seasonMonitorToggleTimeouts[id];

    if (seasonMonitorToggleTimeout) {
      clearTimeout(seasonMonitorToggleTimeout);
      delete seasonMonitorToggleTimeouts[id];
    }

    const series = getState().series.items.find((s) => s.id === id);
    const seasons = _.cloneDeep(series.seasons);
    const season = seasons.find((s) => s.seasonNumber === seasonNumber);

    season.isSaving = true;

    dispatch(updateItem({
      id,
      section,
      seasons
    }));

    seasonsToUpdate[seasonNumber] = monitored;
    season.monitored = monitored;

    seasonMonitorToggleTimeouts[id] = setTimeout(() => {
      createAjaxRequest({
        url: `/series/${id}`,
        method: 'PUT',
        data: JSON.stringify({
          ...series,
          seasons
        }),
        dataType: 'json'
      }).request.then(
        (data) => {
          const changedSeasons = [];

          data.seasons.forEach((s) => {
            if (seasonsToUpdate.hasOwnProperty(s.seasonNumber)) {
              if (s.monitored === seasonsToUpdate[s.seasonNumber]) {
                changedSeasons.push(s);
              } else {
                s.isSaving = true;
              }
            }
          });

          const episodesToUpdate = getState().episodes.items.reduce((acc, episode) => {
            if (episode.seriesId !== data.id) {
              return acc;
            }

            const changedSeason = changedSeasons.find((s) => s.seasonNumber === episode.seasonNumber);

            if (!changedSeason) {
              return acc;
            }

            acc.push(updateItem({
              id: episode.id,
              section: 'episodes',
              monitored: changedSeason.monitored
            }));

            return acc;
          }, []);

          dispatch(batchActions([
            updateItem({
              id,
              section,
              ...data
            }),

            ...episodesToUpdate
          ]));

          changedSeasons.forEach((s) => {
            delete seasonsToUpdate[s.seasonNumber];
          });
        },
        (xhr) => {
          dispatch(updateItem({
            id,
            section,
            seasons: series.seasons
          }));

          Object.keys(seasonsToUpdate).forEach((s) => {
            delete seasonsToUpdate[s];
          });
        });
    }, MONITOR_TIMEOUT);
  },

  [UPDATE_SERIES_MONITOR]: function(getState, payload, dispatch) {
    const {
      seriesIds,
      monitor,
      monitored,
      shouldFetchEpisodesAfterUpdate = false
    } = payload;

    const series = [];

    seriesIds.forEach((id) => {
      const seriesToUpdate = { id };

      if (monitored != null) {
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
      if (shouldFetchEpisodesAfterUpdate) {
        dispatch(fetchEpisodes({ seriesId: seriesIds[0] }));
      }

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
  },

  [SAVE_SERIES_EDITOR]: function(getState, payload, dispatch) {
    dispatch(set({
      section,
      isSaving: true
    }));

    const promise = createAjaxRequest({
      url: '/series/editor',
      method: 'PUT',
      data: JSON.stringify(payload),
      dataType: 'json'
    }).request;

    promise.done((data) => {
      dispatch(batchActions([
        ...data.map((series) => {

          const {
            alternateTitles,
            images,
            rootFolderPath,
            statistics,
            ...propsToUpdate
          } = series;

          return updateItem({
            id: series.id,
            section: 'series',
            ...propsToUpdate
          });
        }),

        set({
          section,
          isSaving: false,
          saveError: null
        })
      ]));
    });

    promise.fail((xhr) => {
      dispatch(set({
        section,
        isSaving: false,
        saveError: xhr
      }));
    });
  },

  [BULK_DELETE_SERIES]: function(getState, payload, dispatch) {
    dispatch(set({
      section,
      isDeleting: true
    }));

    const promise = createAjaxRequest({
      url: '/series/editor',
      method: 'DELETE',
      data: JSON.stringify(payload),
      dataType: 'json'
    }).request;

    promise.done(() => {
      // SignaR will take care of removing the series from the collection

      dispatch(set({
        section,
        isDeleting: false,
        deleteError: null
      }));
    });

    promise.fail((xhr) => {
      dispatch(set({
        section,
        isDeleting: false,
        deleteError: xhr
      }));
    });
  }
});

//
// Reducers

export const reducers = createHandleActions({

  [SET_SERIES_VALUE]: createSetSettingValueReducer(section),

  [SET_DELETE_OPTION]: (state, { payload }) => {
    return {
      ...state,
      deleteOptions: {
        ...payload
      }
    };
  }

}, defaultState, section);
