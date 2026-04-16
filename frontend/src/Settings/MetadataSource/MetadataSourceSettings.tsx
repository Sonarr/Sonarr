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
import {
  MetadataSourceType,
  useManageMetadataSourceSettings,
} from './useMetadataSourceSettings';

const metadataSourceOptions = [
  {
    key: MetadataSourceType.Tvdb,
    value: 'TheTVDB',
  },
  {
    key: MetadataSourceType.Tmdb,
    value: 'TMDb',
  },
];

function MetadataSourceSettings() {
  const {
    isFetching,
    isFetched,
    error,
    hasPendingChanges,
    hasSettings,
    settings,
    isSaving,
    validationErrors,
    validationWarnings,
    saveSettings,
    updateSetting,
  } = useManageMetadataSourceSettings();

  const handleInputChange = useCallback(
    (change: InputChanged) => {
      updateSetting(
        change.name as 'metadataSource' | 'tmdbApiKey',
        change.value as MetadataSourceType | string
      );
    },
    [updateSetting]
  );

  const handleSavePress = useCallback(() => {
    saveSettings();
  }, [saveSettings]);

  const selectedMetadataSource = settings.metadataSource?.value;

  return (
    <PageContent title={translate('MetadataSourceSettings')}>
      <SettingsToolbar
        hasPendingChanges={hasPendingChanges}
        isSaving={isSaving}
        onSavePress={handleSavePress}
      />

      <PageContentBody>
        {isFetching && isFetched ? <LoadingIndicator /> : null}

        {!isFetching && error ? (
          <Alert kind={kinds.DANGER}>
            Failed to load metadata source settings.
          </Alert>
        ) : null}

        {hasSettings && isFetched && !error ? (
          <Form
            id="metadataSourceSettings"
            validationErrors={validationErrors}
            validationWarnings={validationWarnings}
          >
            <FieldSet legend={translate('MetadataSource')}>
              <FormGroup>
                <FormLabel>Provider</FormLabel>

                <FormInputGroup
                  type={inputTypes.SELECT}
                  name="metadataSource"
                  values={metadataSourceOptions}
                  helpText="Choose which provider Sonarr uses for series lookup and refresh metadata."
                  onChange={handleInputChange}
                  {...settings.metadataSource}
                />
              </FormGroup>

              {selectedMetadataSource === MetadataSourceType.Tmdb ? (
                <FormGroup>
                  <FormLabel>TMDb API Key</FormLabel>

                  <FormInputGroup
                    type={inputTypes.PASSWORD}
                    name="tmdbApiKey"
                    helpText="Required for direct TMDb access. Sonarr currently still expects TMDb results to resolve to a TVDB ID for full compatibility."
                    onChange={handleInputChange}
                    {...settings.tmdbApiKey}
                  />
                </FormGroup>
              ) : null}
            </FieldSet>

            <FieldSet legend="Attribution">
              <Alert kind={kinds.INFO}>TheTVDB: https://thetvdb.com</Alert>

              <Alert kind={kinds.INFO}>TMDb: https://www.themoviedb.org</Alert>
            </FieldSet>
          </Form>
        ) : null}
      </PageContentBody>
    </PageContent>
  );
}

export default MetadataSourceSettings;
