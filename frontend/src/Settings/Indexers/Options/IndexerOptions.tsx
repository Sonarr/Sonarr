import React, { useCallback, useEffect } from 'react';
import Alert from 'Components/Alert';
import FieldSet from 'Components/FieldSet';
import Form from 'Components/Form/Form';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import { inputTypes, kinds } from 'Helpers/Props';
import { useShowAdvancedSettings } from 'Settings/advancedSettingsStore';
import { InputChanged } from 'typings/inputs';
import {
  OnChildStateChange,
  SetChildSave,
} from 'typings/Settings/SettingsState';
import translate from 'Utilities/String/translate';
import { useManageIndexerSettings } from './useIndexerSettings';

interface IndexerOptionsProps {
  setChildSave: SetChildSave;
  onChildStateChange: OnChildStateChange;
}

function IndexerOptions({
  setChildSave,
  onChildStateChange,
}: IndexerOptionsProps) {
  const {
    isFetching,
    isFetched,
    isSaving,
    error,
    settings,
    hasSettings,
    hasPendingChanges,
    saveSettings,
    updateSetting,
  } = useManageIndexerSettings();

  const showAdvancedSettings = useShowAdvancedSettings();

  const handleInputChange = useCallback(
    ({ name, value }: InputChanged) => {
      // @ts-expect-error - InputChanged name/value are not typed as keyof IndexerSettingsModel
      updateSetting(name, value);
    },
    [updateSetting]
  );

  useEffect(() => {
    setChildSave(saveSettings);
  }, [saveSettings, setChildSave]);

  useEffect(() => {
    onChildStateChange({
      isSaving,
      hasPendingChanges,
    });
  }, [hasPendingChanges, isSaving, onChildStateChange]);

  return (
    <FieldSet legend={translate('Options')}>
      {isFetching ? <LoadingIndicator /> : null}

      {!isFetching && error ? (
        <Alert kind={kinds.DANGER}>
          {translate('IndexerOptionsLoadError')}
        </Alert>
      ) : null}

      {hasSettings && isFetched && !error ? (
        <Form>
          <FormGroup>
            <FormLabel>{translate('MinimumAge')}</FormLabel>

            <FormInputGroup
              type={inputTypes.NUMBER}
              name="minimumAge"
              min={0}
              unit="minutes"
              helpText={translate('MinimumAgeHelpText')}
              onChange={handleInputChange}
              {...settings.minimumAge}
            />
          </FormGroup>

          <FormGroup>
            <FormLabel>{translate('Retention')}</FormLabel>

            <FormInputGroup
              type={inputTypes.NUMBER}
              name="retention"
              min={0}
              unit="days"
              helpText={translate('RetentionHelpText')}
              onChange={handleInputChange}
              {...settings.retention}
            />
          </FormGroup>

          <FormGroup>
            <FormLabel>{translate('MaximumSize')}</FormLabel>

            <FormInputGroup
              type={inputTypes.NUMBER}
              name="maximumSize"
              min={0}
              unit="MB"
              helpText={translate('MaximumSizeHelpText')}
              onChange={handleInputChange}
              {...settings.maximumSize}
            />
          </FormGroup>

          <FormGroup advancedSettings={showAdvancedSettings} isAdvanced={true}>
            <FormLabel>{translate('RssSyncInterval')}</FormLabel>

            <FormInputGroup
              type={inputTypes.NUMBER}
              name="rssSyncInterval"
              min={0}
              max={120}
              unit="minutes"
              helpText={translate('RssSyncIntervalHelpText')}
              helpTextWarning={translate('RssSyncIntervalHelpTextWarning')}
              helpLink="https://wiki.servarr.com/sonarr/faq#how-does-sonarr-find-episodes"
              onChange={handleInputChange}
              {...settings.rssSyncInterval}
            />
          </FormGroup>
        </Form>
      ) : null}
    </FieldSet>
  );
}

export default IndexerOptions;
