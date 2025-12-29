import { useEffect, useMemo } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';
import { useTranslations } from 'App/useTranslations';
import useCommands from 'Commands/useCommands';
import useCustomFilters from 'Filters/useCustomFilters';
import useSeries from 'Series/useSeries';
import { useUiSettings } from 'Settings/UI/useUiSettings';
import { fetchCustomFilters } from 'Store/Actions/customFilterActions';
import {
  fetchImportLists,
  fetchIndexerFlags,
  fetchLanguages,
  fetchQualityProfiles,
} from 'Store/Actions/settingsActions';
import useSystemStatus from 'System/Status/useSystemStatus';
import useTags from 'Tags/useTags';
import { ApiError } from 'Utilities/Fetch/fetchJson';

const createErrorsSelector = ({
  customFiltersError,
  systemStatusError,
  tagsError,
  translationsError,
  uiSettingsError,
  seriesError,
}: {
  customFiltersError: ApiError | null;
  systemStatusError: ApiError | null;
  tagsError: ApiError | null;
  translationsError: ApiError | null;
  uiSettingsError: ApiError | null;
  seriesError: ApiError | null;
}) =>
  createSelector(
    (state: AppState) => state.settings.qualityProfiles.error,
    (state: AppState) => state.settings.languages.error,
    (state: AppState) => state.settings.importLists.error,
    (state: AppState) => state.settings.indexerFlags.error,
    (
      qualityProfilesError,
      languagesError,
      importListsError,
      indexerFlagsError
    ) => {
      const hasError = !!(
        customFiltersError ||
        seriesError ||
        uiSettingsError ||
        qualityProfilesError ||
        languagesError ||
        importListsError ||
        indexerFlagsError ||
        systemStatusError ||
        tagsError ||
        translationsError ||
        uiSettingsError
      );

      return {
        hasError,
        errors: {
          seriesError,
          customFiltersError,
          tagsError,
          uiSettingsError,
          qualityProfilesError,
          languagesError,
          importListsError,
          indexerFlagsError,
          systemStatusError,
          translationsError,
        },
      };
    }
  );

const useAppPage = () => {
  const dispatch = useDispatch();

  useCommands();

  const { isFetched: isCustomFiltersFetched, error: customFiltersError } =
    useCustomFilters();

  const { isSuccess: isSeriesFetched, error: seriesError } = useSeries();

  const { isFetched: isSystemStatusFetched, error: systemStatusError } =
    useSystemStatus();

  const { isFetched: isTagsFetched, error: tagsError } = useTags();

  const { isFetched: isTranslationsFetched, error: translationsError } =
    useTranslations();

  const { isFetched: isUiSettingsFetched, error: uiSettingsError } =
    useUiSettings();

  const isAppStatePopulated = useSelector(
    (state: AppState) =>
      state.settings.qualityProfiles.isPopulated &&
      state.settings.languages.isPopulated &&
      state.settings.importLists.isPopulated &&
      state.settings.indexerFlags.isPopulated
  );

  const isPopulated =
    isAppStatePopulated &&
    isCustomFiltersFetched &&
    isSeriesFetched &&
    isSystemStatusFetched &&
    isTagsFetched &&
    isTranslationsFetched &&
    isUiSettingsFetched;

  const { hasError, errors } = useSelector(
    createErrorsSelector({
      customFiltersError,
      seriesError,
      systemStatusError,
      tagsError,
      translationsError,
      uiSettingsError,
    })
  );

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
    dispatch(fetchQualityProfiles());
    dispatch(fetchLanguages());
    dispatch(fetchImportLists());
    dispatch(fetchIndexerFlags());
  }, [dispatch]);

  return useMemo(() => {
    return { errors, hasError, isLocalStorageSupported, isPopulated };
  }, [errors, hasError, isLocalStorageSupported, isPopulated]);
};

export default useAppPage;
