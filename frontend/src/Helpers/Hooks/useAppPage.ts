import { useEffect, useMemo } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';
import { fetchTranslations } from 'Store/Actions/appActions';
import { fetchCustomFilters } from 'Store/Actions/customFilterActions';
import { fetchSeries } from 'Store/Actions/seriesActions';
import {
  fetchImportLists,
  fetchIndexerFlags,
  fetchLanguages,
  fetchQualityProfiles,
  fetchUISettings,
} from 'Store/Actions/settingsActions';
import useSystemStatus from 'System/Status/useSystemStatus';
import useTags from 'Tags/useTags';
import { ApiError } from 'Utilities/Fetch/fetchJson';

const createErrorsSelector = ({
  systemStatusError,
  tagsError,
}: {
  systemStatusError: ApiError | null;
  tagsError: ApiError | null;
}) =>
  createSelector(
    (state: AppState) => state.series.error,
    (state: AppState) => state.customFilters.error,
    (state: AppState) => state.settings.ui.error,
    (state: AppState) => state.settings.qualityProfiles.error,
    (state: AppState) => state.settings.languages.error,
    (state: AppState) => state.settings.importLists.error,
    (state: AppState) => state.settings.indexerFlags.error,
    (state: AppState) => state.app.translations.error,
    (
      seriesError,
      customFiltersError,
      uiSettingsError,
      qualityProfilesError,
      languagesError,
      importListsError,
      indexerFlagsError,
      translationsError
    ) => {
      const hasError = !!(
        seriesError ||
        customFiltersError ||
        uiSettingsError ||
        qualityProfilesError ||
        languagesError ||
        importListsError ||
        indexerFlagsError ||
        systemStatusError ||
        tagsError ||
        translationsError
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
  const { isFetched: isSystemStatusFetched, error: systemStatusError } =
    useSystemStatus();

  const { isFetched: isTagsFetched, error: tagsError } = useTags();

  const isAppStatePopulated = useSelector(
    (state: AppState) =>
      state.series.isPopulated &&
      state.customFilters.isPopulated &&
      state.settings.ui.isPopulated &&
      state.settings.qualityProfiles.isPopulated &&
      state.settings.languages.isPopulated &&
      state.settings.importLists.isPopulated &&
      state.settings.indexerFlags.isPopulated &&
      state.app.translations.isPopulated
  );

  const isPopulated =
    isAppStatePopulated && isSystemStatusFetched && isTagsFetched;

  const { hasError, errors } = useSelector(
    createErrorsSelector({ systemStatusError, tagsError })
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
    dispatch(fetchSeries());
    dispatch(fetchCustomFilters());
    dispatch(fetchQualityProfiles());
    dispatch(fetchLanguages());
    dispatch(fetchImportLists());
    dispatch(fetchIndexerFlags());
    dispatch(fetchUISettings());
    dispatch(fetchTranslations());
  }, [dispatch]);

  return useMemo(() => {
    return { errors, hasError, isLocalStorageSupported, isPopulated };
  }, [errors, hasError, isLocalStorageSupported, isPopulated]);
};

export default useAppPage;
