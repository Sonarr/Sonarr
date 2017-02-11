import { combineReducers } from 'redux';
import { enableBatching } from 'redux-batched-actions';
import { routerReducer } from 'react-router-redux';
import app, { defaultState as defaultappState } from './appReducers';
import addSeries, { defaultState as defaultAddSeriesState } from './addSeriesReducers';
import importSeries, { defaultState as defaultImportSeriesState } from './importSeriesReducers';
import series, { defaultState as defaultSeriesState } from './seriesReducers';
import seriesIndex, { defaultState as defaultSeriesIndexState } from './seriesIndexReducers';
import seriesEditor, { defaultState as defaultSeriesEditorState } from './seriesEditorReducers';
import seasonPass, { defaultState as defaultSeasonPassState } from './seasonPassReducers';
import calendar, { defaultState as defaultCalendarState } from './calendarReducers';
import history, { defaultState as defaultHistoryState } from './historyReducers';
import queue, { defaultState as defaultQueueState } from './queueReducers';
import blacklist, { defaultState as defaultBlacklistState } from './blacklistReducers';
import episodes, { defaultState as defaultEpisodesState } from './episodeReducers';
import episodeFiles, { defaultState as defaultEpisodeFilesState } from './episodeFileReducers';
import episodeHistory, { defaultState as defaultEpisodeHistoryState } from './episodeHistoryReducers';
import releases, { defaultState as defaultReleasesState } from './releaseReducers';
import wanted, { defaultState as defaultWantedState } from './wantedReducers';
import settings, { defaultState as defaultSettingsState } from './settingsReducers';
import system, { defaultState as defaultSystemState } from './systemReducers';
import commands, { defaultState as defaultCommandsState } from './commandReducers';
import paths, { defaultState as defaultPathsState } from './pathReducers';
import tags, { defaultState as defaultTagsState } from './tagReducers';
import captcha, { defaultState as defaultCaptchaState } from './captchaReducers';
import oAuth, { defaultState as defaultOAuthState } from './oAuthReducers';
import interactiveImport, { defaultState as defaultInteractiveImportState } from './interactiveImportReducers';
import rootFolders, { defaultState as defaultRootFoldersState } from './rootFolderReducers';
import organizePreview, { defaultState as defaultOrganizePreviewState } from './organizePreviewReducers';

export const defaultState = {
  app: defaultappState,
  addSeries: defaultAddSeriesState,
  importSeries: defaultImportSeriesState,
  series: defaultSeriesState,
  seriesIndex: defaultSeriesIndexState,
  seriesEditor: defaultSeriesEditorState,
  seasonPass: defaultSeasonPassState,
  calendar: defaultCalendarState,
  history: defaultHistoryState,
  queue: defaultQueueState,
  blacklist: defaultBlacklistState,
  episodes: defaultEpisodesState,
  episodeFiles: defaultEpisodeFilesState,
  episodeHistory: defaultEpisodeHistoryState,
  releases: defaultReleasesState,
  wanted: defaultWantedState,
  settings: defaultSettingsState,
  system: defaultSystemState,
  commands: defaultCommandsState,
  paths: defaultPathsState,
  tags: defaultTagsState,
  captcha: defaultCaptchaState,
  oAuth: defaultOAuthState,
  interactiveImport: defaultInteractiveImportState,
  rootFolders: defaultRootFoldersState,
  organizePreview: defaultOrganizePreviewState
};

export default enableBatching(combineReducers({
  app,
  addSeries,
  importSeries,
  series,
  seriesIndex,
  seriesEditor,
  seasonPass,
  calendar,
  history,
  queue,
  blacklist,
  episodes,
  episodeFiles,
  episodeHistory,
  releases,
  wanted,
  settings,
  system,
  commands,
  paths,
  tags,
  captcha,
  oAuth,
  interactiveImport,
  rootFolders,
  organizePreview,
  routing: routerReducer
}));
