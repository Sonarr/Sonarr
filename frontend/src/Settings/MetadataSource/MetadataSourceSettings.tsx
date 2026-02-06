import React, { useCallback } from 'react';
import FieldSet from 'Components/FieldSet';
import Form from 'Components/Form/Form';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import { inputTypes } from 'Helpers/Props';
import SettingsToolbar from 'Settings/SettingsToolbar';
import { InputChanged } from 'typings/inputs';
import translate from 'Utilities/String/translate';
import { useManageMetadataSourceSettings } from './useMetadataSourceSettings';
import TheTvdb from './TheTvdb';

export const tvdbMetadataLanguageOptions = [
  { key: 'en', value: translate('TvdbMetadataLanguageEnglish') },
  { key: 'de', value: translate('TvdbMetadataLanguageGerman') },
  { key: 'fr', value: translate('TvdbMetadataLanguageFrench') },
  { key: 'es', value: translate('TvdbMetadataLanguageSpanish') },
  { key: 'it', value: translate('TvdbMetadataLanguageItalian') },
  { key: 'tr', value: translate('TvdbMetadataLanguageTurkish') },
  { key: 'pt', value: translate('TvdbMetadataLanguagePortuguese') },
  { key: 'nl', value: translate('TvdbMetadataLanguageDutch') },
  { key: 'pl', value: translate('TvdbMetadataLanguagePolish') },
  { key: 'ru', value: translate('TvdbMetadataLanguageRussian') },
  { key: 'ja', value: translate('TvdbMetadataLanguageJapanese') },
  { key: 'zh', value: translate('TvdbMetadataLanguageChinese') },
  { key: 'sv', value: translate('TvdbMetadataLanguageSwedish') },
  { key: 'da', value: translate('TvdbMetadataLanguageDanish') },
  { key: 'fi', value: translate('TvdbMetadataLanguageFinnish') },
  { key: 'no', value: translate('TvdbMetadataLanguageNorwegian') },
  { key: 'ko', value: translate('TvdbMetadataLanguageKorean') },
  { key: 'cs', value: translate('TvdbMetadataLanguageCzech') },
  { key: 'hu', value: translate('TvdbMetadataLanguageHungarian') },
  { key: 'el', value: translate('TvdbMetadataLanguageGreek') },
  { key: 'ro', value: translate('TvdbMetadataLanguageRomanian') },
  { key: 'th', value: translate('TvdbMetadataLanguageThai') },
  { key: 'uk', value: translate('TvdbMetadataLanguageUkrainian') },
  { key: 'id', value: translate('TvdbMetadataLanguageIndonesian') },
  { key: 'ms', value: translate('TvdbMetadataLanguageMalay') },
  { key: 'he', value: translate('TvdbMetadataLanguageHebrew') },
  { key: 'ar', value: translate('TvdbMetadataLanguageArabic') },
  { key: 'hi', value: translate('TvdbMetadataLanguageHindi') },
];

function MetadataSourceSettings() {
  const {
    isFetching,
    isFetched,
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
        change.name as keyof import('./useMetadataSourceSettings').MetadataSourceSettingsModel,
        change.value
      );
    },
    [updateSetting]
  );

  const handleSavePress = useCallback(() => {
    saveSettings();
  }, [saveSettings]);

  return (
    <PageContent title={translate('MetadataSourceSettings')}>
      <SettingsToolbar
        hasPendingChanges={hasPendingChanges}
        isSaving={isSaving}
        onSavePress={handleSavePress}
      />

      <PageContentBody>
        {isFetching && !isFetched ? <LoadingIndicator /> : null}

        <TheTvdb />

        {hasSettings && isFetched ? (
          <Form
            id="metadataSourceSettings"
            validationErrors={validationErrors}
            validationWarnings={validationWarnings}
          >
            <FieldSet legend={translate('TvdbMetadataLanguageSettings')}>
              <FormGroup>
                <FormLabel>{translate('TvdbMetadataLanguage')}</FormLabel>
                <FormInputGroup
                  type={inputTypes.SELECT}
                  name="tvdbMetadataLanguage"
                  values={tvdbMetadataLanguageOptions}
                  helpText={translate('TvdbMetadataLanguageHelpText')}
                  onChange={handleInputChange}
                  {...settings.tvdbMetadataLanguage}
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
