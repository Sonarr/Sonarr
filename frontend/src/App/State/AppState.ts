import { Error } from './AppSectionState';
import BlocklistAppState from './BlocklistAppState';
import CaptchaAppState from './CaptchaAppState';
import CommandAppState from './CommandAppState';
import HistoryAppState, { SeriesHistoryAppState } from './HistoryAppState';
import ImportSeriesAppState from './ImportSeriesAppState';
import InteractiveImportAppState from './InteractiveImportAppState';
import MessagesAppState from './MessagesAppState';
import OAuthAppState from './OAuthAppState';
import OrganizePreviewAppState from './OrganizePreviewAppState';
import ProviderOptionsAppState from './ProviderOptionsAppState';
import SettingsAppState from './SettingsAppState';

export interface AppSectionState {
  isUpdated: boolean;
  isConnected: boolean;
  isDisconnected: boolean;
  isReconnecting: boolean;
  isRestarting: boolean;
  isSidebarVisible: boolean;
  version: string;
  prevVersion?: string;
  dimensions: {
    isSmallScreen: boolean;
    isLargeScreen: boolean;
    width: number;
    height: number;
  };
  translations: {
    error?: Error;
    isPopulated: boolean;
  };
  messages: MessagesAppState;
}

interface AppState {
  app: AppSectionState;
  blocklist: BlocklistAppState;
  captcha: CaptchaAppState;
  commands: CommandAppState;
  episodeHistory: HistoryAppState;
  importSeries: ImportSeriesAppState;
  interactiveImport: InteractiveImportAppState;
  oAuth: OAuthAppState;
  organizePreview: OrganizePreviewAppState;
  providerOptions: ProviderOptionsAppState;
  seriesHistory: SeriesHistoryAppState;
  settings: SettingsAppState;
}

export default AppState;
