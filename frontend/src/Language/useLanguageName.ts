import moment from 'moment';
import { useCallback, useEffect } from 'react';
import useApiQuery from 'Helpers/Hooks/useApiQuery';

interface LanguageResponse {
  identifier: string;
}

function getDisplayName(code: string) {
  return Intl.DisplayNames
    ? new Intl.DisplayNames([code], { type: 'language' })
    : null;
}

export const useLanguage = () => {
  return useApiQuery<LanguageResponse>({
    path: '/localization/language',
    queryOptions: {
      staleTime: Infinity,
      gcTime: Infinity,
    },
  });
};

export const useInitializeLanguage = () => {
  const { data } = useLanguage();

  useEffect(() => {
    moment.locale(data?.identifier);
  }, [data]);
};

const useLanguageName = () => {
  const { data } = useLanguage();

  const getLanguageName = useCallback(
    (code: string): string => {
      const languageNames = data?.identifier
        ? getDisplayName(data.identifier)
        : getDisplayName('en');

      if (!languageNames) {
        return code;
      }

      try {
        return languageNames.of(code) ?? code;
      } catch {
        return code;
      }
    },
    [data]
  );

  return getLanguageName;
};

export default useLanguageName;
