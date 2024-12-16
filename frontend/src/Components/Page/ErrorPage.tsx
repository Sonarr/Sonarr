import React from 'react';
import { Error } from 'App/State/AppSectionState';
import getErrorMessage from 'Utilities/Object/getErrorMessage';
import translate from 'Utilities/String/translate';
import styles from './ErrorPage.css';

interface ErrorPageProps {
  version: string;
  isLocalStorageSupported: boolean;
  translationsError?: Error;
  seriesError?: Error;
  customFiltersError?: Error;
  tagsError?: Error;
  qualityProfilesError?: Error;
  uiSettingsError?: Error;
  systemStatusError?: Error;
}

function ErrorPage(props: ErrorPageProps) {
  const {
    version,
    isLocalStorageSupported,
    translationsError,
    seriesError,
    customFiltersError,
    tagsError,
    qualityProfilesError,
    uiSettingsError,
    systemStatusError,
  } = props;

  let errorMessage = translate('FailedToLoadSonarr');

  if (!isLocalStorageSupported) {
    errorMessage = translate('LocalStorageIsNotSupported');
  } else if (translationsError) {
    errorMessage = getErrorMessage(
      translationsError,
      translate('FailedToLoadTranslationsFromApi')
    );
  } else if (seriesError) {
    errorMessage = getErrorMessage(
      seriesError,
      translate('FailedToLoadSeriesFromApi')
    );
  } else if (customFiltersError) {
    errorMessage = getErrorMessage(
      customFiltersError,
      translate('FailedToLoadCustomFiltersFromApi')
    );
  } else if (tagsError) {
    errorMessage = getErrorMessage(
      tagsError,
      translate('FailedToLoadTagsFromApi')
    );
  } else if (qualityProfilesError) {
    errorMessage = getErrorMessage(
      qualityProfilesError,
      translate('FailedToLoadQualityProfilesFromApi')
    );
  } else if (uiSettingsError) {
    errorMessage = getErrorMessage(
      uiSettingsError,
      translate('FailedToLoadUiSettingsFromApi')
    );
  } else if (systemStatusError) {
    errorMessage = getErrorMessage(
      systemStatusError,
      translate('FailedToLoadSystemStatusFromApi')
    );
  }

  return (
    <div className={styles.page}>
      <div>{errorMessage}</div>

      <div className={styles.version}>
        {translate('VersionNumber', { version })}
      </div>
    </div>
  );
}

export default ErrorPage;
