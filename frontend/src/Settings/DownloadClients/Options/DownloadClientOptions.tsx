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
import { useManageDownloadClientSettings } from './useDownloadClientSettings';

interface DownloadClientOptionsProps {
  setChildSave: SetChildSave;
  onChildStateChange: OnChildStateChange;
}

function DownloadClientOptions({
  setChildSave,
  onChildStateChange,
}: DownloadClientOptionsProps) {
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
  } = useManageDownloadClientSettings();

  const showAdvancedSettings = useShowAdvancedSettings();

  const handleInputChange = useCallback(
    ({ name, value }: InputChanged) => {
      // @ts-expect-error - InputChanged name/value are not typed as keyof DownloadClientSettingsModel
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
    <div>
      {isFetching ? <LoadingIndicator /> : null}

      {!isFetching && error ? (
        <Alert kind={kinds.DANGER}>
          {translate('DownloadClientOptionsLoadError')}
        </Alert>
      ) : null}

      {hasSettings && isFetched && !error && showAdvancedSettings ? (
        <FieldSet
          legend={translate('CompletedDownloadHandling')}
          caption={translate('CompletedDownloadHandlingCaption')}
        >
          <Form>
            <FormRow advancedSettings={showAdvancedSettings} isAdvanced={true}>
              <FormLabel>{translate('Enable')}</FormLabel>

              <FormInputHelpText
                text={translate('EnableCompletedDownloadHandlingHelpText')}
              />

              <FormInput
                type={inputTypes.CHECK}
                name="enableCompletedDownloadHandling"
                onChange={handleInputChange}
                {...settings.enableCompletedDownloadHandling}
              />
            </FormRow>

            <FormRow advancedSettings={showAdvancedSettings} isAdvanced={true}>
              <FormLabel>{translate('AutoRedownloadFailed')}</FormLabel>

              <FormInputHelpText
                text={translate('AutoRedownloadFailedHelpText')}
              />

              <FormInput
                type={inputTypes.CHECK}
                name="autoRedownloadFailed"
                onChange={handleInputChange}
                {...settings.autoRedownloadFailed}
              />
            </FormRow>

            {settings.autoRedownloadFailed.value ? (
              <FormRow
                advancedSettings={showAdvancedSettings}
                isAdvanced={true}
              >
                <FormLabel>
                  {translate('AutoRedownloadFailedFromInteractiveSearch')}
                </FormLabel>

                <FormInputHelpText
                  text={translate(
                    'AutoRedownloadFailedFromInteractiveSearchHelpText'
                  )}
                />

                <FormInput
                  type={inputTypes.CHECK}
                  name="autoRedownloadFailedFromInteractiveSearch"
                  onChange={handleInputChange}
                  {...settings.autoRedownloadFailedFromInteractiveSearch}
                />
              </FormRow>
            ) : null}
          </Form>
        </FieldSet>
      ) : null}
    </div>
  );
}

export default DownloadClientOptions;
