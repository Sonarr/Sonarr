import React, { useCallback, useEffect } from 'react';
import Alert from 'Components/Alert';
import FieldSet from 'Components/FieldSet';
import Form from 'Components/Form/Form';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import { inputTypes, kinds, sizes } from 'Helpers/Props';
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
        <FieldSet legend={translate('CompletedDownloadHandling')}>
          <Form>
            <FormGroup
              advancedSettings={showAdvancedSettings}
              isAdvanced={true}
              size={sizes.MEDIUM}
            >
              <FormLabel>{translate('Enable')}</FormLabel>

              <FormInputGroup
                type={inputTypes.CHECK}
                name="enableCompletedDownloadHandling"
                helpText={translate('EnableCompletedDownloadHandlingHelpText')}
                onChange={handleInputChange}
                {...settings.enableCompletedDownloadHandling}
              />
            </FormGroup>

            <FormGroup
              advancedSettings={showAdvancedSettings}
              isAdvanced={true}
              size={sizes.MEDIUM}
            >
              <FormLabel>{translate('AutoRedownloadFailed')}</FormLabel>

              <FormInputGroup
                type={inputTypes.CHECK}
                name="autoRedownloadFailed"
                helpText={translate('AutoRedownloadFailedHelpText')}
                onChange={handleInputChange}
                {...settings.autoRedownloadFailed}
              />
            </FormGroup>

            {settings.autoRedownloadFailed.value ? (
              <FormGroup
                advancedSettings={showAdvancedSettings}
                isAdvanced={true}
                size={sizes.MEDIUM}
              >
                <FormLabel>
                  {translate('AutoRedownloadFailedFromInteractiveSearch')}
                </FormLabel>

                <FormInputGroup
                  type={inputTypes.CHECK}
                  name="autoRedownloadFailedFromInteractiveSearch"
                  helpText={translate(
                    'AutoRedownloadFailedFromInteractiveSearchHelpText'
                  )}
                  onChange={handleInputChange}
                  {...settings.autoRedownloadFailedFromInteractiveSearch}
                />
              </FormGroup>
            ) : null}
          </Form>
        </FieldSet>
      ) : null}
    </div>
  );
}

export default DownloadClientOptions;
