import { useEffect, useMemo } from 'react';
import { useDispatch } from 'react-redux';
import { useTranslations } from 'App/useTranslations';
import useCommands from 'Commands/useCommands';
import useCustomFilters from 'Filters/useCustomFilters';
import { useInitializeLanguage } from 'Language/useLanguageName';
import { useLanguages } from 'Language/useLanguages';
import useSeries from 'Series/useSeries';
import useIndexerFlags from 'Settings/Indexers/useIndexerFlags';
import { useQualityProfiles } from 'Settings/Profiles/Quality/useQualityProfiles';
import { useUiSettings } from 'Settings/UI/useUiSettings';
import { fetchCustomFilters } from 'Store/Actions/customFilterActions';
import useSystemStatus from 'System/Status/useSystemStatus';
import useTags from 'Tags/useTags';

const useAppPage = () => {
  const dispatch = useDispatch();

  useCommands();
  useInitializeLanguage();

  const { isFetched: isCustomFiltersFetched, error: customFiltersError } =
    useCustomFilters();

  const { isFetched: isSeriesFetched, error: seriesError } = useSeries();

  const { isFetched: isSystemStatusFetched, error: systemStatusError } =
    useSystemStatus();

  const { isFetched: isTagsFetched, error: tagsError } = useTags();

  const { isFetched: isTranslationsFetched, error: translationsError } =
    useTranslations();

  const { isFetched: isUiSettingsFetched, error: uiSettingsError } =
    useUiSettings();

  const { isFetched: isQualityProfilesFetched, error: qualityProfilesError } =
    useQualityProfiles();

  const { isFetched: isLanguagesFetched, error: languagesError } =
    useLanguages();

  const { isFetched: isIndexerFlagsFetched, error: indexerFlagsError } =
    useIndexerFlags();

  const isPopulated =
    isCustomFiltersFetched &&
    isIndexerFlagsFetched &&
    isSeriesFetched &&
    isSystemStatusFetched &&
    isTagsFetched &&
    isTranslationsFetched &&
    isUiSettingsFetched &&
    isQualityProfilesFetched &&
    isLanguagesFetched;

  const { hasError, errors } = useMemo(() => {
    return {
      hasError: !!(
        customFiltersError ||
        seriesError ||
        uiSettingsError ||
        qualityProfilesError ||
        languagesError ||
        indexerFlagsError ||
        systemStatusError ||
        tagsError ||
        translationsError
      ),
      errors: {
        seriesError,
        customFiltersError,
        tagsError,
        uiSettingsError,
        qualityProfilesError,
        languagesError,
        indexerFlagsError,
        systemStatusError,
        translationsError,
      },
    };
  }, [
    customFiltersError,
    seriesError,
    uiSettingsError,
    qualityProfilesError,
    languagesError,
    indexerFlagsError,
    systemStatusError,
    tagsError,
    translationsError,
  ]);

  const isLocalStorageSupported = useMemo(() => {
    const key = 'sonarrTest';

    try {
      localStorage.setItem(key, key);
      localStorage.removeItem(key);

      return true;
    } catch {
      return false;
    }
  }, []);

  useEffect(() => {
    dispatch(fetchCustomFilters());
  }, [dispatch]);

  return useMemo(() => {
    return { errors, hasError, isLocalStorageSupported, isPopulated };
  }, [errors, hasError, isLocalStorageSupported, isPopulated]);
};

export default useAppPage;
