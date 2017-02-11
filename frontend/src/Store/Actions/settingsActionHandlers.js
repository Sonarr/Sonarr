import _ from 'lodash';
import $ from 'jquery';
import { batchActions } from 'redux-batched-actions';
import * as types from './actionTypes';
import createFetchHandler from './Creators/createFetchHandler';
import createFetchSchemaHandler from './Creators/createFetchSchemaHandler';
import createSaveHandler from './Creators/createSaveHandler';
import createSaveProviderHandler, { createCancelSaveProviderHandler } from './Creators/createSaveProviderHandler';
import createRemoveItemHandler from './Creators/createRemoveItemHandler';
import createTestProviderHandler, { createCancelTestProviderHandler } from './Creators/createTestProviderHandler';
import { set, update, clearPendingChanges } from './baseActions';

const settingsActionHandlers = {
  [types.FETCH_UI_SETTINGS]: createFetchHandler('ui', '/config/ui'),
  [types.SAVE_UI_SETTINGS]: createSaveHandler('ui', '/config/ui', (state) => state.settings.ui),

  [types.FETCH_MEDIA_MANAGEMENT_SETTINGS]: createFetchHandler('mediaManagement', '/config/mediamanagement'),
  [types.SAVE_MEDIA_MANAGEMENT_SETTINGS]: createSaveHandler('mediaManagement', '/config/mediamanagement', (state) => state.settings.mediaManagement),

  [types.FETCH_NAMING_SETTINGS]: createFetchHandler('naming', '/config/naming'),
  [types.SAVE_NAMING_SETTINGS]: createSaveHandler('naming', '/config/naming', (state) => state.settings.naming),

  [types.FETCH_NAMING_EXAMPLES]: function(payload) {
    const section = 'namingExamples';

    return function(dispatch, getState) {
      dispatch(set({ section, isFetching: true }));

      const naming = getState().settings.naming;

      const promise = $.ajax({
        url: '/config/naming/examples',
        data: Object.assign({}, naming.item, naming.pendingChanges)
      });

      promise.done((data) => {
        dispatch(batchActions([
          update({ section, data }),

          set({
            section,
            isFetching: false,
            isPopulated: true,
            error: null
          })
        ]));
      });

      promise.fail((xhr) => {
        dispatch(set({
          section,
          isFetching: false,
          isPopulated: false,
          error: xhr
        }));
      });
    };
  },

  [types.REORDER_DELAY_PROFILE]: function(payload) {
    const section = 'delayProfiles';

    return function(dispatch, getState) {
      const { id, moveIndex } = payload;
      const moveOrder = moveIndex + 1;
      const delayProfiles = getState().settings.delayProfiles.items;
      const moving = _.find(delayProfiles, { id });

      // Don't move if the order hasn't changed
      if (moving.order === moveOrder) {
        return;
      }

      const after = moveIndex > 0 ? _.find(delayProfiles, { order: moveIndex }) : null;
      const afterQueryParam = after ? `after=${after.id}` : '';

      const promise = $.ajax({
        method: 'PUT',
        url: `/delayprofile/reorder/${id}?${afterQueryParam}`
      });

      promise.done((data) => {
        dispatch(update({ section, data }));
      });
    };
  },

  [types.FETCH_QUALITY_PROFILES]: createFetchHandler('qualityProfiles', '/qualityprofile'),
  [types.FETCH_QUALITY_PROFILE_SCHEMA]: createFetchSchemaHandler('qualityProfiles', '/qualityprofile/schema'),

  [types.SAVE_QUALITY_PROFILE]: createSaveProviderHandler(
    'qualityProfiles',
    '/qualityprofile',
    (state) => state.settings.qualityProfiles),

  [types.DELETE_QUALITY_PROFILE]: createRemoveItemHandler(
    'qualityProfiles',
    '/qualityprofile',
    (state) => state.settings.qualityProfiles),

  [types.FETCH_LANGUAGE_PROFILES]: createFetchHandler('languageProfiles', '/languageprofile'),
  [types.FETCH_LANGUAGE_PROFILE_SCHEMA]: createFetchSchemaHandler('languageProfiles', '/languageprofile/schema'),

  [types.SAVE_LANGUAGE_PROFILE]: createSaveProviderHandler('languageProfiles',
    '/languageprofile',
    (state) => state.settings.languageProfiles),

  [types.DELETE_LANGUAGE_PROFILE]: createRemoveItemHandler('languageProfiles',
    '/languageprofile',
    (state) => state.settings.languageProfiles),

  [types.FETCH_DELAY_PROFILES]: createFetchHandler('delayProfiles', '/delayprofile'),

  [types.SAVE_DELAY_PROFILE]: createSaveProviderHandler('delayProfiles',
    '/delayprofile',
    (state) => state.settings.delayProfiles),

  [types.DELETE_DELAY_PROFILE]: createRemoveItemHandler('delayProfiles',
    '/delayprofile',
    (state) => state.settings.delayProfiles),

  [types.FETCH_QUALITY_DEFINITIONS]: createFetchHandler('qualityDefinitions', '/qualitydefinition'),
  [types.SAVE_QUALITY_DEFINITIONS]: createSaveHandler('qualityDefinitions', '/qualitydefinition', (state) => state.settings.qualitydefinitions),

  [types.SAVE_QUALITY_DEFINITIONS]: function() {
    const section = 'qualityDefinitions';

    return function(dispatch, getState) {
      const qualityDefinitions = getState().settings.qualityDefinitions;

      const upatedDefinitions = Object.keys(qualityDefinitions.pendingChanges).map((key) => {
        const id = parseInt(key);
        const pendingChanges = qualityDefinitions.pendingChanges[id] || {};
        const item = _.find(qualityDefinitions.items, { id });

        return Object.assign({}, item, pendingChanges);
      });

      // If there is nothing to save don't bother isSaving
      if (!upatedDefinitions || !upatedDefinitions.length) {
        return;
      }

      const promise = $.ajax({
        method: 'PUT',
        url: '/qualityDefinition/update',
        data: JSON.stringify(upatedDefinitions)
      });

      promise.done((data) => {
        dispatch(batchActions([
          update({ section, data }),
          clearPendingChanges({ section: 'qualityDefinitions' })
        ]));
      });
    };
  },

  [types.FETCH_INDEXERS]: createFetchHandler('indexers', '/indexer'),
  [types.FETCH_INDEXER_SCHEMA]: createFetchSchemaHandler('indexers', '/indexer/schema'),

  [types.SAVE_INDEXER]: createSaveProviderHandler('indexers',
    '/indexer',
    (state) => state.settings.indexers),

  [types.CANCEL_SAVE_INDEXER]: createCancelSaveProviderHandler('indexers'),

  [types.DELETE_INDEXER]: createRemoveItemHandler('indexers',
    '/indexer',
    (state) => state.settings.indexers),

  [types.TEST_INDEXER]: createTestProviderHandler('indexers',
    '/indexer',
    (state) => state.settings.indexers),

  [types.CANCEL_TEST_INDEXER]: createCancelTestProviderHandler('indexers'),

  [types.FETCH_INDEXER_OPTIONS]: createFetchHandler('indexerOptions', '/config/indexer'),
  [types.SAVE_INDEXER_OPTIONS]: createSaveHandler('indexerOptions', '/config/indexer', (state) => state.settings.indexerOptions),

  [types.FETCH_RESTRICTIONS]: createFetchHandler('restrictions', '/restriction'),

  [types.SAVE_RESTRICTION]: createSaveProviderHandler('restrictions',
    '/restriction',
    (state) => state.settings.restrictions),

  [types.DELETE_RESTRICTION]: createRemoveItemHandler('restrictions',
    '/restriction',
    (state) => state.settings.restrictions),

  [types.FETCH_DOWNLOAD_CLIENTS]: createFetchHandler('downloadClients', '/downloadclient'),
  [types.FETCH_DOWNLOAD_CLIENT_SCHEMA]: createFetchSchemaHandler('downloadClients', '/downloadclient/schema'),

  [types.SAVE_DOWNLOAD_CLIENT]: createSaveProviderHandler('downloadClients',
    '/downloadclient',
    (state) => state.settings.downloadClients),

  [types.CANCEL_SAVE_DOWNLOAD_CLIENT]: createCancelSaveProviderHandler('downloadClients'),

  [types.DELETE_DOWNLOAD_CLIENT]: createRemoveItemHandler('downloadClients',
    '/downloadclient',
    (state) => state.settings.downloadClients),

  [types.TEST_DOWNLOAD_CLIENT]: createTestProviderHandler('downloadClients',
    '/downloadclient',
    (state) => state.settings.downloadClients),

  [types.CANCEL_TEST_DOWNLOAD_CLIENT]: createCancelTestProviderHandler('downloadClients'),

  [types.FETCH_DOWNLOAD_CLIENT_OPTIONS]: createFetchHandler('downloadClientOptions', '/config/downloadclient'),
  [types.SAVE_DOWNLOAD_CLIENT_OPTIONS]: createSaveHandler('downloadClientOptions', '/config/downloadclient', (state) => state.settings.downloadClientOptions),

  [types.FETCH_REMOTE_PATH_MAPPINGS]: createFetchHandler('remotePathMappings', '/remotepathmapping'),

  [types.SAVE_REMOTE_PATH_MAPPING]: createSaveProviderHandler(
    'remotePathMappings',
    '/remotepathmapping',
    (state) => state.settings.remotePathMappings),

  [types.DELETE_REMOTE_PATH_MAPPING]: createRemoveItemHandler(
    'remotePathMappings',
    '/remotepathmapping',
    (state) => state.settings.remotePathMappings),

  [types.FETCH_NOTIFICATIONS]: createFetchHandler('notifications', '/notification'),
  [types.FETCH_NOTIFICATION_SCHEMA]: createFetchSchemaHandler('notifications', '/notification/schema'),

  [types.SAVE_NOTIFICATION]: createSaveProviderHandler('notifications',
    '/notification',
    (state) => state.settings.notifications),

  [types.CANCEL_SAVE_NOTIFICATION]: createCancelSaveProviderHandler('notifications'),

  [types.DELETE_NOTIFICATION]: createRemoveItemHandler('notifications',
    '/notification',
    (state) => state.settings.notifications),

  [types.TEST_NOTIFICATION]: createTestProviderHandler(
    'notifications',
    '/notification',
    (state) => state.settings.notifications),

  [types.CANCEL_TEST_NOTIFICATION]: createCancelTestProviderHandler('notifications'),

  [types.FETCH_METADATA]: createFetchHandler('metadata', '/metadata'),

  [types.SAVE_METADATA]: createSaveProviderHandler(
    'metadata',
    '/metadata',
    (state) => state.settings.metadata),

  [types.FETCH_GENERAL_SETTINGS]: createFetchHandler('general', '/config/host'),
  [types.SAVE_GENERAL_SETTINGS]: createSaveHandler('general', '/config/host', (state) => state.settings.general)
};

export default settingsActionHandlers;
