import React, { useCallback } from 'react';
import Alert from 'Components/Alert';
import FieldSet from 'Components/FieldSet';
import Form from 'Components/Form/Form';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import { inputTypes, kinds } from 'Helpers/Props';
import SettingsToolbar from 'Settings/SettingsToolbar';
import { InputChanged } from 'typings/inputs';
import translate from 'Utilities/String/translate';
import TheTvdb from './TheTvdb';
import { useManageMetadataSourceSettings } from './useMetadataSource';
import styles from './TheTvdb.css';

function MetadataSourceSettings() {
  const {
    isFetching,
    isFetched,
    error,
    hasPendingChanges,
    hasSettings,
    settings,
    isSaving,
    saveSettings,
    updateSetting,
    validationErrors,
    validationWarnings,
  } = useManageMetadataSourceSettings();

  const handleInputChange = useCallback(
    (change: InputChanged) => {
      // @ts-expect-error name needs to be keyof MetadataSourceSettingsModel
      updateSetting(change.name, change.value);
    },
    [updateSetting]
  );

  const handleSavePress = useCallback(() => {
    saveSettings();
  }, [saveSettings]);

  const isPopulated = isFetched;

  return (
    <PageContent title={translate('MetadataSourceSettings')}>
      <SettingsToolbar
        hasPendingChanges={hasPendingChanges}
        isSaving={isSaving}
        onSavePress={handleSavePress}
      />

      <PageContentBody>
        <div className={styles.container}>
          <TheTvdb />
        </div>

        {isFetching && isPopulated ? <LoadingIndicator /> : null}

        {!isFetching && error ? (
          <Alert kind={kinds.DANGER}>
            {translate('MetadataSourceSettingsLoadError')}
          </Alert>
        ) : null}

        {hasSettings && isPopulated && !error ? (
          <Form
            id="metadataSourceSettings"
            validationErrors={validationErrors}
            validationWarnings={validationWarnings}
          >
            <FieldSet legend={translate('TheTvdb')}>
              <FormGroup>
                <FormLabel>{translate('TmdbApiKey')}</FormLabel>

                <FormInputGroup
                  type={inputTypes.PASSWORD}
                  name="tmdbApiKey"
                  helpText={translate('TmdbApiKeyHelpText')}
                  helpLink="https://www.themoviedb.org/settings/api"
                  onChange={handleInputChange}
                  {...settings.tmdbApiKey}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>{translate('TmdbEnabled')}</FormLabel>

                <FormInputGroup
                  type={inputTypes.CHECK}
                  name="tmdbEnabled"
                  helpText={translate('TmdbEnabledHelpText')}
                  onChange={handleInputChange}
                  {...settings.tmdbEnabled}
                />
              </FormGroup>
            </FieldSet>
          </Form>
        ) : null}
      </PageContentBody>
    </PageContent>
  );
}

export default MetadataSourceSettings;
