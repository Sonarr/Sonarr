import * as app from './appActions';
import * as captcha from './captchaActions';
import * as commands from './commandActions';
import * as customFilters from './customFilterActions';
import * as episodes from './episodeActions';
import * as episodeFiles from './episodeFileActions';
import * as episodeHistory from './episodeHistoryActions';
import * as episodeSelection from './episodeSelectionActions';
import * as importSeries from './importSeriesActions';
import * as interactiveImportActions from './interactiveImportActions';
import * as oAuth from './oAuthActions';
import * as organizePreview from './organizePreviewActions';
import * as paths from './pathActions';
import * as providerOptions from './providerOptionActions';
import * as releases from './releaseActions';
import * as rootFolders from './rootFolderActions';
import * as series from './seriesActions';
import * as seriesHistory from './seriesHistoryActions';
import * as seriesIndex from './seriesIndexActions';
import * as settings from './settingsActions';

export default [
  app,
  captcha,
  commands,
  customFilters,
  episodes,
  episodeFiles,
  episodeHistory,
  episodeSelection,
  importSeries,
  interactiveImportActions,
  oAuth,
  organizePreview,
  paths,
  providerOptions,
  releases,
  rootFolders,
  series,
  seriesHistory,
  seriesIndex,
  settings
];
