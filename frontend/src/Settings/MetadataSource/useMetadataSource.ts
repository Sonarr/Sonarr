import { useCallback } from 'react';
import {
  useManageSettings,
  useSaveSettings,
  useSettings,
} from 'Settings/useSettings';

export interface MetadataSourceSettingsModel {
  tmdbApiKey: string;
  tmdbEnabled: boolean;
}

const PATH = '/settings/metadatasource';

export const useMetadataSourceSettingsValues = () => {
  const { data } = useSettings<MetadataSourceSettingsModel>(PATH);

  return data;
};

export const useMetadataSourceSettings = () => {
  return useSettings<MetadataSourceSettingsModel>(PATH);
};

export const useManageMetadataSourceSettings = () => {
  return useManageSettings<MetadataSourceSettingsModel>(PATH);
};

export const useSaveMetadataSourceSettings = () => {
  const { data } = useSettings<MetadataSourceSettingsModel>(PATH);
  const { save } = useSaveSettings<MetadataSourceSettingsModel>(PATH);

  const saveSettings = useCallback(
    (changes: Partial<MetadataSourceSettingsModel>) => {
      const updatedSettings = { ...data, ...changes };

      save(updatedSettings);
    },
    [data, save]
  );

  return saveSettings;
};
