import _ from 'lodash';
import React from 'react';
import { createAction } from 'redux-actions';
import { batchActions } from 'redux-batched-actions';
import Icon from 'Components/Icon';
import episodeEntities from 'Episode/episodeEntities';
import { icons, sortDirections } from 'Helpers/Props';
import { createThunk, handleThunks } from 'Store/thunks';
import createAjaxRequest from 'Utilities/createAjaxRequest';
import translate from 'Utilities/String/translate';
import { updateItem } from './baseActions';
import createFetchHandler from './Creators/createFetchHandler';
import createHandleActions from './Creators/createHandleActions';
import createSetClientSideCollectionSortReducer from './Creators/Reducers/createSetClientSideCollectionSortReducer';
import createSetTableOptionReducer from './Creators/Reducers/createSetTableOptionReducer';

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
      columnLabel: () => translate('Monitored'),
      isVisible: true,
      isModifiable: false
    },
    {
      name: 'episodeNumber',
      label: '#',
      isVisible: true,
      isSortable: true
    },
    {
      name: 'title',
      label: () => translate('Title'),
      isVisible: true,
      isSortable: true
    },
    {
      name: 'path',
      label: () => translate('Path'),
      isVisible: false,
      isSortable: true
    },
    {
      name: 'relativePath',
      label: () => translate('RelativePath'),
      isVisible: false,
      isSortable: true
    },
    {
      name: 'airDateUtc',
      label: () => translate('AirDate'),
      isVisible: true,
      isSortable: true
    },
    {
      name: 'runtime',
      label: () => translate('Runtime'),
      isVisible: false,
      isSortable: true
    },
    {
      name: 'languages',
      label: () => translate('Languages'),
      isVisible: false
    },
    {
      name: 'audioInfo',
      label: () => translate('AudioInfo'),
      isVisible: false
    },
    {
      name: 'videoCodec',
      label: () => translate('VideoCodec'),
      isVisible: false
    },
    {
      name: 'videoDynamicRangeType',
      label: () => translate('VideoDynamicRange'),
      isVisible: false
    },
    {
      name: 'audioLanguages',
      label: () => translate('AudioLanguages'),
      isVisible: false
    },
    {
      name: 'subtitleLanguages',
      label: () => translate('SubtitleLanguages'),
      isVisible: false
    },
    {
      name: 'size',
      label: () => translate('Size'),
      isVisible: false,
      isSortable: true
    },
    {
      name: 'releaseGroup',
      label: () => translate('ReleaseGroup'),
      isVisible: false
    },
    {
      name: 'customFormats',
      label: () => translate('Formats'),
      isVisible: false
    },
    {
      name: 'customFormatScore',
      columnLabel: () => translate('CustomFormatScore'),
      label: React.createElement(Icon, {
        name: icons.SCORE,
        title: () => translate('CustomFormatScore')
      }),
      isVisible: false,
      isSortable: true
    },
    {
      name: 'status',
      label: () => translate('Status'),
      isVisible: true
    },
    {
      name: 'actions',
      columnLabel: () => translate('Actions'),
      isVisible: true,
      isModifiable: false
    }
  ]
};

export const persistState = [
  'episodes.columns',
  'episodes.sortDirection',
  'episodes.sortKey'
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

    dispatch(updateItem({
      id,
      section: episodeEntity,
      isSaving: true
    }));

    const promise = createAjaxRequest({
      url: `/episode/${id}`,
      method: 'PUT',
      data: JSON.stringify({ monitored }),
      dataType: 'json'
    }).request;

    promise.done((data) => {
      dispatch(updateItem({
        id,
        section: episodeEntity,
        isSaving: false,
        monitored
      }));
    });

    promise.fail((xhr) => {
      dispatch(updateItem({
        id,
        section: episodeEntity,
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

    const promise = createAjaxRequest({
      url: '/episode/monitor',
      method: 'PUT',
      data: JSON.stringify({ episodeIds, monitored }),
      dataType: 'json'
    }).request;

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
