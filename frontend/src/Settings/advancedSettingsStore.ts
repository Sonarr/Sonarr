import { createPersist } from 'Helpers/createPersist';

interface AdvancedSettingsState {
  showAdvancedSettings: boolean;
}

const advancedSettingsStore = createPersist<AdvancedSettingsState>(
  'advanced_settings',
  () => ({ showAdvancedSettings: false })
);

export const useShowAdvancedSettings = () => {
  return advancedSettingsStore((state) => state.showAdvancedSettings);
};

export const toggleShowAdvancedSettings = () => {
  advancedSettingsStore.setState((state) => ({
    showAdvancedSettings: !state.showAdvancedSettings,
  }));
};
