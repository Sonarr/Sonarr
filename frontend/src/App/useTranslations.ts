import { useEffect } from 'react';
import useApiQuery from 'Helpers/Hooks/useApiQuery';
import { setTranslations } from 'Utilities/String/translate';

interface TranslationsResponse {
  strings: Record<string, string>;
}

export function useTranslations() {
  const { data, ...result } = useApiQuery<TranslationsResponse>({
    path: '/localization',
    queryOptions: {
      staleTime: Infinity,
      gcTime: Infinity,
    },
  });

  useEffect(() => {
    if (data) {
      setTranslations(data.strings);
    }
  }, [data]);

  return {
    ...result,
    data,
  };
}
