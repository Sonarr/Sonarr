import _ from 'lodash';
import $ from 'jquery';
import { batchActions } from 'redux-batched-actions';
import episodeEntities from 'Episode/episodeEntities';
import createFetchHandler from './Creators/createFetchHandler';
import createRemoveItemHandler from './Creators/createRemoveItemHandler';
import * as types from './actionTypes';
import { set, removeItem, updateItem } from './baseActions';

const section = 'episodeFiles';
const deleteEpisodeFile = createRemoveItemHandler(section, '/episodeFile');

const episodeFileActionHandlers = {
  [types.FETCH_EPISODE_FILES]: createFetchHandler(section, '/episodeFile'),

  [types.DELETE_EPISODE_FILE]: function(payload) {
    return function(dispatch, getState) {
      const {
        id: episodeFileId,
        episodeEntity = episodeEntities.EPISODES
      } = payload;

      const episodeSection = _.last(episodeEntity.split('.'));

      const deletePromise = deleteEpisodeFile(payload)(dispatch, getState);

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
    };
  },

  [types.DELETE_EPISODE_FILES]: function(payload) {
    return function(dispatch, getState) {
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
    };
  },

  [types.UPDATE_EPISODE_FILES]: function(payload) {
    return function(dispatch, getState) {
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
    };
  }
};

export default episodeFileActionHandlers;
