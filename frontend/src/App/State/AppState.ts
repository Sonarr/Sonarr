import BlocklistAppState from './BlocklistAppState';
import CaptchaAppState from './CaptchaAppState';
import HistoryAppState, { SeriesHistoryAppState } from './HistoryAppState';
import ImportSeriesAppState from './ImportSeriesAppState';
import InteractiveImportAppState from './InteractiveImportAppState';
import OAuthAppState from './OAuthAppState';
import OrganizePreviewAppState from './OrganizePreviewAppState';
import ProviderOptionsAppState from './ProviderOptionsAppState';
import SettingsAppState from './SettingsAppState';

interface AppState {
  blocklist: BlocklistAppState;
  captcha: CaptchaAppState;
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
