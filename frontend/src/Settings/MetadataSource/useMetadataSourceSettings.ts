import { useManageSettings, useSettings } from 'Settings/useSettings';

export enum MetadataSourceType {
  Tvdb = 0,
  Tmdb = 1,
}

export interface MetadataSourceSettingsModel {
  metadataSource: MetadataSourceType;
  tmdbApiKey: string;
}

const PATH = '/settings/metadatasource';

export const useMetadataSourceSettings = () => {
  return useSettings<MetadataSourceSettingsModel>(PATH);
};

export const useManageMetadataSourceSettings = () => {
  return useManageSettings<MetadataSourceSettingsModel>(PATH);
};
