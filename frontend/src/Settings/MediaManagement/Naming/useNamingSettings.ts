import { keepPreviousData } from '@tanstack/react-query';
import { useMemo } from 'react';
import useApiQuery from 'Helpers/Hooks/useApiQuery';
import { useManageSettings, useSettings } from 'Settings/useSettings';
import { PendingSection } from 'typings/pending';
import { QueryParams } from 'Utilities/Fetch/getQueryString';

const PATH = '/settings/naming';
const EXAMPLES_PATH = '/settings/naming/examples';

export interface NamingSettingsModel {
  renameEpisodes: boolean;
  replaceIllegalCharacters: boolean;
  colonReplacementFormat: number;
  customColonReplacementFormat: string;
  multiEpisodeStyle: number;
  standardEpisodeFormat: string;
  dailyEpisodeFormat: string;
  animeEpisodeFormat: string;
  seriesFolderFormat: string;
  seasonFolderFormat: string;
  specialsFolderFormat: string;
}

export interface NamingExamples {
  singleEpisodeExample: string;
  multiEpisodeExample: string;
  dailyEpisodeExample: string;
  animeEpisodeExample: string;
  animeMultiEpisodeExample: string;
  seriesFolderExample: string;
  seasonFolderExample: string;
  specialsFolderExample: string;
}

export const useNamingSettings = () => {
  return useSettings<NamingSettingsModel>(PATH);
};

export const useManageNamingSettings = () => {
  return useManageSettings<NamingSettingsModel>(PATH);
};

export const useNamingExamples = (
  settings: PendingSection<NamingSettingsModel>
) => {
  const queryParams = useMemo<QueryParams>(() => {
    return Object.entries(settings).reduce((acc, [key, value]) => {
      if (typeof value === 'object' && 'value' in value) {
        acc[key] = value.value;
      }

      return acc;
    }, {} as QueryParams);
  }, [settings]);

  const { data, error, isFetching } = useApiQuery<NamingExamples>({
    path: EXAMPLES_PATH,
    method: 'GET',
    queryParams,
    queryOptions: {
      placeholderData: keepPreviousData,
    },
  });

  return {
    examples: data,
    isExamplesFetching: isFetching,
    examplesError: error,
  };
};
