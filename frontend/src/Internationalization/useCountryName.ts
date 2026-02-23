import { useMemo } from 'react';
import { useLanguage } from 'Language/useLanguageName';
import getCountryCode from './getCountryCode';

const useCountryName = (countryCode: string | undefined) => {
  const { data } = useLanguage();

  return useMemo(() => {
    if (!countryCode) {
      return '';
    }

    const locale = data?.identifier ?? 'en';

    const getDisplayName = Intl.DisplayNames
      ? new Intl.DisplayNames([locale], { type: 'region', fallback: 'code' })
      : null;

    if (!getDisplayName) {
      return countryCode;
    }

    try {
      return getDisplayName.of(getCountryCode(countryCode)) ?? countryCode;
    } catch (e) {
      console.warn('Error getting country name for code:', countryCode, e);
      return countryCode;
    }
  }, [countryCode, data]);
};

export default useCountryName;
