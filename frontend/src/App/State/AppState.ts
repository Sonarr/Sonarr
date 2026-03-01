import CaptchaAppState from './CaptchaAppState';
import SettingsAppState from './SettingsAppState';

interface AppState {
  captcha: CaptchaAppState;
  settings: SettingsAppState;
}

export default AppState;
