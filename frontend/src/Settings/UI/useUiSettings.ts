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
const QUERY_OPTIONS = { staleTime: 60 * 60 * 1000 };

export const useUiSettingsValues = () => {
  const { data } = useSettings<UiSettingsModel>(PATH, QUERY_OPTIONS);

  return data;
};

export const useUiSettings = () => {
  return useSettings<UiSettingsModel>(PATH, QUERY_OPTIONS);
};

export const useManageUiSettings = () => {
  return useManageSettings<UiSettingsModel>(PATH);
};

export const useSaveUiSettings = () => {
  const { data } = useSettings<UiSettingsModel>(PATH, QUERY_OPTIONS);
  const { save } = useSaveSettings<UiSettingsModel>(PATH);

  const saveSettings = useCallback(
    (changes: Partial<UiSettingsModel>) => {
      const updatedSettings = { ...data, ...changes };

      save(updatedSettings);
    },
    [data, save]
  );

  return saveSettings;
};
