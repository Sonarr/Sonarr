import { useSelector } from 'react-redux';
import AppState from 'App/State/AppState';

function useShowAdvancedSettings() {
  return useSelector((state: AppState) => state.settings.advancedSettings);
}

export default useShowAdvancedSettings;
