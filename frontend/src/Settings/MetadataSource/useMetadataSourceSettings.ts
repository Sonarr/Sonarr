import {
  useManageSettings,
  useSaveSettings,
  useSettings,
} from 'Settings/useSettings';

export interface MetadataSourceSettingsModel {
  tvdbMetadataLanguage: string;
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

  return {
    saveSettings: (changes: Partial<MetadataSourceSettingsModel>) => {
      save({ ...data, ...changes });
    },
  };
};
