import React, { useCallback, useEffect } from 'react';
import Alert from 'Components/Alert';
import FieldSet from 'Components/FieldSet';
import Form from 'Components/Form/Form';
import FormInput from 'Components/Form/FormInput';
import FormInputHelpText from 'Components/Form/FormInputHelpText';
import FormLabel from 'Components/Form/FormLabel';
import FormRow from 'Components/Form/FormRow';
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
    <FieldSet
      legend={translate('Options')}
      caption={translate('IndexerOptionsCaption')}
    >
      {isFetching ? <LoadingIndicator /> : null}
      {!isFetching && error ? (
        <Alert kind={kinds.DANGER}>
          {translate('IndexerOptionsLoadError')}
        </Alert>
      ) : null}
      {hasSettings && isFetched && !error ? (
        <Form>
          <FormRow>
            <FormLabel>{translate('MinimumAge')}</FormLabel>
            <FormInputHelpText text={translate('MinimumAgeHelpText')} />
            <FormInput
              type={inputTypes.NUMBER}
              name="minimumAge"
              min={0}
              unit="minutes"
              onChange={handleInputChange}
              {...settings.minimumAge}
            />
          </FormRow>

          <FormRow>
            <FormLabel>{translate('Retention')}</FormLabel>
            <FormInputHelpText text={translate('RetentionHelpText')} />
            <FormInput
              type={inputTypes.NUMBER}
              name="retention"
              min={0}
              unit="days"
              onChange={handleInputChange}
              {...settings.retention}
            />
          </FormRow>

          <FormRow>
            <FormLabel>{translate('MaximumSize')}</FormLabel>
            <FormInputHelpText text={translate('MaximumSizeHelpText')} />
            <FormInput
              type={inputTypes.NUMBER}
              name="maximumSize"
              min={0}
              unit="MB"
              onChange={handleInputChange}
              {...settings.maximumSize}
            />
          </FormRow>

          <FormRow advancedSettings={showAdvancedSettings} isAdvanced={true}>
            <FormLabel>{translate('RssSyncInterval')}</FormLabel>
            <FormInputHelpText
              text={translate('RssSyncIntervalHelpText')}
              link="https://wiki.servarr.com/sonarr/faq#how-does-sonarr-find-episodes"
            />
            <FormInputHelpText
              text={translate('RssSyncIntervalHelpTextWarning')}
              isWarning={true}
            />
            <FormInput
              type={inputTypes.NUMBER}
              name="rssSyncInterval"
              min={0}
              max={120}
              unit="minutes"
              onChange={handleInputChange}
              {...settings.rssSyncInterval}
            />
          </FormRow>
        </Form>
      ) : null}
    </FieldSet>
  );
}

export default IndexerOptions;
