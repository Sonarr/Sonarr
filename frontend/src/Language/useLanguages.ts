import { useMemo } from 'react';
import useApiQuery from 'Helpers/Hooks/useApiQuery';
import Language from 'Language/Language';

interface LanguageFilter {
  [key: string]: boolean | undefined;
  includeAny: boolean;
  includeOriginal?: boolean;
  includeUnknown?: boolean;
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
  excludeLanguages: LanguageFilter = { includeAny: true }
) => {
  const { data, isFetching, isFetched, error } = useLanguages();

  const filteredItems = useMemo(() => {
    if (!data) return [];

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

export const useLanguageByName = (name: string | undefined) => {
  const { data } = useLanguages();

  return useMemo(() => {
    if (!name || !data) {
      return undefined;
    }

    return data.find((language) => language.name === name);
  }, [data, name]);
};
