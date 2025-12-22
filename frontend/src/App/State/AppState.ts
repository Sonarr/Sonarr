import BlocklistAppState from './BlocklistAppState';
import CaptchaAppState from './CaptchaAppState';
import ImportSeriesAppState from './ImportSeriesAppState';
import ProviderOptionsAppState from './ProviderOptionsAppState';
import SettingsAppState from './SettingsAppState';

interface AppState {
  blocklist: BlocklistAppState;
  captcha: CaptchaAppState;
  importSeries: ImportSeriesAppState;
  providerOptions: ProviderOptionsAppState;
  settings: SettingsAppState;
}

export default AppState;
