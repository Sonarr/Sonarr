import _ from 'lodash';
import $ from 'jquery';
import { createAction } from 'redux-actions';
import { batchActions } from 'redux-batched-actions';
import { createThunk, handleThunks } from 'Store/thunks';
import episodeEntities from 'Episode/episodeEntities';
import createFetchHandler from './Creators/createFetchHandler';
import createHandleActions from './Creators/createHandleActions';
import createRemoveItemHandler from './Creators/createRemoveItemHandler';
import { set, removeItem, updateItem } from './baseActions';

//
// Variables

export const section = 'episodeFiles';

//
// State

export const defaultState = {
  isFetching: false,
  isPopulated: false,
  error: null,
  isDeleting: false,
  deleteError: null,
  isSaving: false,
  saveError: null,
  items: []
};

//
// Actions Types

export const FETCH_EPISODE_FILES = 'episodeFiles/fetchEpisodeFiles';
export const DELETE_EPISODE_FILE = 'episodeFiles/deleteEpisodeFile';
export const DELETE_EPISODE_FILES = 'episodeFiles/deleteEpisodeFiles';
export const UPDATE_EPISODE_FILES = 'episodeFiles/updateEpisodeFiles';
export const CLEAR_EPISODE_FILES = 'episodeFiles/clearEpisodeFiles';

//
// Action Creators

export const fetchEpisodeFiles = createThunk(FETCH_EPISODE_FILES);
export const deleteEpisodeFile = createThunk(DELETE_EPISODE_FILE);
export const deleteEpisodeFiles = createThunk(DELETE_EPISODE_FILES);
export const updateEpisodeFiles = createThunk(UPDATE_EPISODE_FILES);
export const clearEpisodeFiles = createAction(CLEAR_EPISODE_FILES);

//
// Helpers

const deleteEpisodeFileHelper = createRemoveItemHandler(section, '/episodeFile');

//
// Action Handlers

export const actionHandlers = handleThunks({
  [FETCH_EPISODE_FILES]: createFetchHandler(section, '/episodeFile'),

  [DELETE_EPISODE_FILE]: function(getState, payload, dispatch) {
    const {
      id: episodeFileId,
      episodeEntity = episodeEntities.EPISODES
    } = payload;

    const episodeSection = _.last(episodeEntity.split('.'));
    const deletePromise = deleteEpisodeFileHelper(getState, payload, dispatch);

    deletePromise.done(() => {
      const episodes = getState().episodes.items;
      const episodesWithRemovedFiles = _.filter(episodes, { episodeFileId });

      dispatch(batchActions([
        ...episodesWithRemovedFiles.map((episode) => {
          return updateItem({
            section: episodeSection,
            ...episode,
            episodeFileId: 0,
            hasFile: false
          });
        })
      ]));
    });
  },

  [DELETE_EPISODE_FILES]: function(getState, payload, dispatch) {
    const {
      episodeFileIds
    } = payload;

    dispatch(set({ section, isDeleting: true }));

    const promise = $.ajax({
      url: '/episodeFile/bulk',
      method: 'DELETE',
      dataType: 'json',
      data: JSON.stringify({ episodeFileIds })
    });

    promise.done(() => {
      const episodes = getState().episodes.items;
      const episodesWithRemovedFiles = episodeFileIds.reduce((acc, episodeFileId) => {
        acc.push(..._.filter(episodes, { episodeFileId }));

        return acc;
      }, []);

      dispatch(batchActions([
        ...episodeFileIds.map((id) => {
          return removeItem({ section, id });
        }),

        ...episodesWithRemovedFiles.map((episode) => {
          return updateItem({
            section: 'episodes',
            ...episode,
            episodeFileId: 0,
            hasFile: false
          });
        }),

        set({
          section,
          isDeleting: false,
          deleteError: null
        })
      ]));
    });

    promise.fail((xhr) => {
      dispatch(set({
        section,
        isDeleting: false,
        deleteError: xhr
      }));
    });
  },

  [UPDATE_EPISODE_FILES]: function(getState, payload, dispatch) {
    const {
      episodeFileIds,
      language,
      quality
    } = payload;

    dispatch(set({ section, isSaving: true }));

    const data = {
      episodeFileIds
    };

    if (language) {
      data.language = language;
    }

    if (quality) {
      data.quality = quality;
    }

    const promise = $.ajax({
      url: '/episodeFile/editor',
      method: 'PUT',
      dataType: 'json',
      data: JSON.stringify(data)
    });

    promise.done(() => {
      dispatch(batchActions([
        ...episodeFileIds.map((id) => {
          const props = {};

          if (language) {
            props.language = language;
          }

          if (quality) {
            props.quality = quality;
          }

          return updateItem({ section, id, ...props });
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
  }
});

//
// Reducers

export const reducers = createHandleActions({

  [CLEAR_EPISODE_FILES]: (state) => {
    return Object.assign({}, state, defaultState);
  }

}, defaultState, section);
