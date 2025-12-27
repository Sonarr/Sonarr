import CaptchaAppState from './CaptchaAppState';
import ImportSeriesAppState from './ImportSeriesAppState';
import SettingsAppState from './SettingsAppState';

interface AppState {
  captcha: CaptchaAppState;
  importSeries: ImportSeriesAppState;
  settings: SettingsAppState;
}

export default AppState;
