import _ from 'lodash';
import $ from 'jquery';
import { createAction } from 'redux-actions';
import { batchActions } from 'redux-batched-actions';
import { sortDirections } from 'Helpers/Props';
import { createThunk, handleThunks } from 'Store/thunks';
import createSetClientSideCollectionSortReducer from './Creators/Reducers/createSetClientSideCollectionSortReducer';
import createSetTableOptionReducer from './Creators/Reducers/createSetTableOptionReducer';
import episodeEntities from 'Episode/episodeEntities';
import createFetchHandler from './Creators/createFetchHandler';
import createHandleActions from './Creators/createHandleActions';
import { updateItem } from './baseActions';

//
// Variables

export const section = 'episodes';

//
// State

export const defaultState = {
  isFetching: false,
  isPopulated: false,
  error: null,
  sortKey: 'episodeNumber',
  sortDirection: sortDirections.DESCENDING,
  items: [],

  columns: [
    {
      name: 'monitored',
      columnLabel: 'Monitored',
      isVisible: true,
      isModifiable: false
    },
    {
      name: 'episodeNumber',
      label: '#',
      isVisible: true
    },
    {
      name: 'title',
      label: 'Title',
      isVisible: true
    },
    {
      name: 'path',
      label: 'Path',
      isVisible: false
    },
    {
      name: 'relativePath',
      label: 'Relative Path',
      isVisible: false
    },
    {
      name: 'airDateUtc',
      label: 'Air Date',
      isVisible: true
    },
    {
      name: 'language',
      label: 'Language',
      isVisible: false
    },
    {
      name: 'audioInfo',
      label: 'Audio Info',
      isVisible: false
    },
    {
      name: 'videoCodec',
      label: 'Video Codec',
      isVisible: false
    },
    {
      name: 'status',
      label: 'Status',
      isVisible: true
    },
    {
      name: 'actions',
      columnLabel: 'Actions',
      isVisible: true,
      isModifiable: false
    }
  ]
};

export const persistState = [
  'episodes.columns'
];

//
// Actions Types

export const FETCH_EPISODES = 'episodes/fetchEpisodes';
export const SET_EPISODES_SORT = 'episodes/setEpisodesSort';
export const SET_EPISODES_TABLE_OPTION = 'episodes/setEpisodesTableOption';
export const CLEAR_EPISODES = 'episodes/clearEpisodes';
export const TOGGLE_EPISODE_MONITORED = 'episodes/toggleEpisodeMonitored';
export const TOGGLE_EPISODES_MONITORED = 'episodes/toggleEpisodesMonitored';

//
// Action Creators

export const fetchEpisodes = createThunk(FETCH_EPISODES);
export const setEpisodesSort = createAction(SET_EPISODES_SORT);
export const setEpisodesTableOption = createAction(SET_EPISODES_TABLE_OPTION);
export const clearEpisodes = createAction(CLEAR_EPISODES);
export const toggleEpisodeMonitored = createThunk(TOGGLE_EPISODE_MONITORED);
export const toggleEpisodesMonitored = createThunk(TOGGLE_EPISODES_MONITORED);

//
// Action Handlers

export const actionHandlers = handleThunks({
  [FETCH_EPISODES]: createFetchHandler(section, '/episode'),

  [TOGGLE_EPISODE_MONITORED]: function(getState, payload, dispatch) {
    const {
      episodeId: id,
      episodeEntity = episodeEntities.EPISODES,
      monitored
    } = payload;

    const episodeSection = _.last(episodeEntity.split('.'));

    dispatch(updateItem({
      id,
      section: episodeSection,
      isSaving: true
    }));

    const promise = $.ajax({
      url: `/episode/${id}`,
      method: 'PUT',
      data: JSON.stringify({ monitored }),
      dataType: 'json'
    });

    promise.done((data) => {
      dispatch(updateItem({
        id,
        section: episodeSection,
        isSaving: false,
        monitored
      }));
    });

    promise.fail((xhr) => {
      dispatch(updateItem({
        id,
        section: episodeSection,
        isSaving: false
      }));
    });
  },

  [TOGGLE_EPISODES_MONITORED]: function(getState, payload, dispatch) {
    const {
      episodeIds,
      episodeEntity = episodeEntities.EPISODES,
      monitored
    } = payload;

    const episodeSection = _.last(episodeEntity.split('.'));

    dispatch(batchActions(
      episodeIds.map((episodeId) => {
        return updateItem({
          id: episodeId,
          section: episodeSection,
          isSaving: true
        });
      })
    ));

    const promise = $.ajax({
      url: '/episode/monitor',
      method: 'PUT',
      data: JSON.stringify({ episodeIds, monitored }),
      dataType: 'json'
    });

    promise.done((data) => {
      dispatch(batchActions(
        episodeIds.map((episodeId) => {
          return updateItem({
            id: episodeId,
            section: episodeSection,
            isSaving: false,
            monitored
          });
        })
      ));
    });

    promise.fail((xhr) => {
      dispatch(batchActions(
        episodeIds.map((episodeId) => {
          return updateItem({
            id: episodeId,
            section: episodeSection,
            isSaving: false
          });
        })
      ));
    });
  }
});

//
// Reducers

export const reducers = createHandleActions({

  [SET_EPISODES_TABLE_OPTION]: createSetTableOptionReducer(section),

  [CLEAR_EPISODES]: (state) => {
    return Object.assign({}, state, {
      isFetching: false,
      isPopulated: false,
      error: null,
      items: []
    });
  },

  [SET_EPISODES_SORT]: createSetClientSideCollectionSortReducer(section)

}, defaultState, section);
