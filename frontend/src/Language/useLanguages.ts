import { useMemo } from 'react';
import useApiQuery from 'Helpers/Hooks/useApiQuery';
import Language, { DEFAULT_LANGUAGE } from 'Language/Language';
import { useMetadataSourceSettingsValues } from 'Settings/MetadataSource/useMetadataSourceSettings';

interface LanguageFilter {
  [key: string]: boolean | undefined;
  Any: boolean;
  Original?: boolean;
  Unknown?: boolean;
}

const PATH = '/language';

export const useLanguages = () => {
  return useApiQuery<Language[]>({
    path: PATH,
    queryOptions: {
      gcTime: Infinity,
      staleTime: Infinity,
    },
  });
};

export const useFilteredLanguages = (
  excludeLanguages: LanguageFilter = { Any: true }
) => {
  const { data, isFetching, isFetched, error } = useLanguages();

  const filteredItems = useMemo(() => {
    if (!data) {
      return [];
    }

    return data.filter((lang) => !excludeLanguages[lang.name]);
  }, [data, excludeLanguages]);

  return {
    data: filteredItems,
    isFetching,
    isFetched,
    error,
  };
};

export const useLanguageById = (id: number | undefined) => {
  const { data } = useLanguages();

  return useMemo(() => {
    if (id === undefined || !data) {
      return undefined;
    }

    return data.find((language) => language.id === id);
  }, [data, id]);
};

export const usePreferredMetadataLanguage = () => {
  const { preferredMetadataLanguage } = useMetadataSourceSettingsValues();
  const language = useLanguageById(preferredMetadataLanguage);

  return language ?? DEFAULT_LANGUAGE;
};

export const useLanguageByName = (name: string | undefined) => {
  const { data } = useLanguages();

  return useMemo(() => {
    if (!name || !data) {
      return undefined;
    }

    return data.find((language) => language.name === name);
  }, [data, name]);
};
