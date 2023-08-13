import PropTypes from 'prop-types';
import React from 'react';
import getErrorMessage from 'Utilities/Object/getErrorMessage';
import translate from 'Utilities/String/translate';
import styles from './ErrorPage.css';

function ErrorPage(props) {
  const {
    version,
    isLocalStorageSupported,
    translationsError,
    seriesError,
    customFiltersError,
    tagsError,
    qualityProfilesError,
    uiSettingsError,
    systemStatusError
  } = props;

  let errorMessage = translate('FailedToLoadSonarr');

  if (!isLocalStorageSupported) {
    errorMessage = translate('LocalStorageIsNotSupported');
  } else if (translationsError) {
    errorMessage = getErrorMessage(translationsError, translate('FailedToLoadTranslationsFromApi'));
  } else if (seriesError) {
    errorMessage = getErrorMessage(seriesError, translate('FailedToLoadSeriesFromApi'));
  } else if (customFiltersError) {
    errorMessage = getErrorMessage(customFiltersError, translate('FailedToLoadCustomFiltersFromApi'));
  } else if (tagsError) {
    errorMessage = getErrorMessage(tagsError, translate('FailedToLoadTagsFromApi'));
  } else if (qualityProfilesError) {
    errorMessage = getErrorMessage(qualityProfilesError, translate('FailedToLoadQualityProfilesFromApi'));
  } else if (uiSettingsError) {
    errorMessage = getErrorMessage(uiSettingsError, translate('FailedToLoadUiSettingsFromApi'));
  } else if (systemStatusError) {
    errorMessage = getErrorMessage(uiSettingsError, translate('FailedToLoadSystemStatusFromApi'));
  }

  return (
    <div className={styles.page}>
      <div className={styles.errorMessage}>
        {errorMessage}
      </div>

      <div className={styles.version}>
        {translate('VersionNumber', { version })}
      </div>
    </div>
  );
}

ErrorPage.propTypes = {
  version: PropTypes.string.isRequired,
  isLocalStorageSupported: PropTypes.bool.isRequired,
  translationsError: PropTypes.object,
  seriesError: PropTypes.object,
  customFiltersError: PropTypes.object,
  tagsError: PropTypes.object,
  qualityProfilesError: PropTypes.object,
  uiSettingsError: PropTypes.object,
  systemStatusError: PropTypes.object
};

export default ErrorPage;
