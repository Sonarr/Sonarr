import { useCallback } from 'react';
import {
  useManageSettings,
  useSaveSettings,
  useSettings,
} from 'Settings/useSettings';

export interface UiSettingsModel {
  theme: 'auto' | 'dark' | 'light';
  showRelativeDates: boolean;
  shortDateFormat: string;
  longDateFormat: string;
  timeFormat: string;
  timeZone: string;
  firstDayOfWeek: number;
  enableColorImpairedMode: boolean;
  calendarWeekColumnHeader: string;
  uiLanguage: number;
}

const PATH = '/settings/ui';

export const useUiSettingsValues = () => {
  const { data } = useSettings<UiSettingsModel>(PATH);

  return data;
};

export const useUiSettings = () => {
  return useSettings<UiSettingsModel>(PATH);
};

export const useManageUiSettings = () => {
  return useManageSettings<UiSettingsModel>(PATH);
};

export const useSaveUiSettings = () => {
  const { data } = useSettings<UiSettingsModel>(PATH);
  const { save } = useSaveSettings<UiSettingsModel>(PATH);

  const saveSettings = useCallback(
    (changes: Partial<UiSettingsModel>) => {
      const updatedSettings = {
        ...data,
        ...changes,
      };

      save(updatedSettings);
    },
    [data, save]
  );

  return saveSettings;
};
