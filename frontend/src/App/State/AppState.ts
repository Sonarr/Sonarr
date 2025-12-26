import BlocklistAppState from './BlocklistAppState';
import CaptchaAppState from './CaptchaAppState';
import ImportSeriesAppState from './ImportSeriesAppState';
import SettingsAppState from './SettingsAppState';

interface AppState {
  blocklist: BlocklistAppState;
  captcha: CaptchaAppState;
  importSeries: ImportSeriesAppState;
  settings: SettingsAppState;
}

export default AppState;
