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

  if (data) {
    setTranslations(data.strings);
  }

  return {
    ...result,
    data,
  };
}
