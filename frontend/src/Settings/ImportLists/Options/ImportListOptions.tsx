import React, { useCallback, useEffect } from 'react';
import Alert from 'Components/Alert';
import FieldSet from 'Components/FieldSet';
import Form from 'Components/Form/Form';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import { EnhancedSelectInputValue } from 'Components/Form/Select/EnhancedSelectInput';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import { inputTypes, kinds } from 'Helpers/Props';
import { useShowAdvancedSettings } from 'Settings/advancedSettingsStore';
import { InputChanged } from 'typings/inputs';
import {
  OnChildStateChange,
  SetChildSave,
} from 'typings/Settings/SettingsState';
import translate from 'Utilities/String/translate';
import { useManageImportListSettings } from './useImportListSettings';

const cleanLibraryLevelOptions: EnhancedSelectInputValue<string>[] = [
  {
    key: 'disabled',
    get value() {
      return translate('Disabled');
    },
  },
  {
    key: 'logOnly',
    get value() {
      return translate('LogOnly');
    },
  },
  {
    key: 'keepAndUnmonitor',
    get value() {
      return translate('KeepAndUnmonitorSeries');
    },
  },
  {
    key: 'keepAndTag',
    get value() {
      return translate('KeepAndTagSeries');
    },
  },
];

interface ImportListOptionsProps {
  setChildSave: SetChildSave;
  onChildStateChange: OnChildStateChange;
}

function ImportListOptions({
  setChildSave,
  onChildStateChange,
}: ImportListOptionsProps) {
  const showAdvancedSettings = useShowAdvancedSettings();

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
  } = useManageImportListSettings();

  const handleInputChange = useCallback(
    ({ name, value }: InputChanged) => {
      // @ts-expect-error - InputChanged name/value are not typed as keyof ImportListSettingsModel
      updateSetting(name, value);
    },
    [updateSetting]
  );

  const handleTagChange = useCallback(
    ({ value }: { name: string; value: number[] }) => {
      const id = value.length === 0 ? 0 : value[value.length - 1];
      updateSetting('listSyncTag', id);
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

  if (!showAdvancedSettings) {
    return null;
  }

  return (
    <FieldSet legend={translate('Options')}>
      {isFetching ? <LoadingIndicator /> : null}

      {!isFetching && error ? (
        <Alert kind={kinds.DANGER}>{translate('ListOptionsLoadError')}</Alert>
      ) : null}

      {hasSettings && isFetched && !error ? (
        <Form>
          <FormGroup advancedSettings={showAdvancedSettings} isAdvanced={true}>
            <FormLabel>{translate('CleanLibraryLevel')}</FormLabel>
            <FormInputGroup
              type={inputTypes.SELECT}
              name="listSyncLevel"
              values={cleanLibraryLevelOptions}
              helpText={translate('ListSyncLevelHelpText')}
              onChange={handleInputChange}
              {...settings.listSyncLevel}
            />
          </FormGroup>

          {settings.listSyncLevel.value === 'keepAndTag' ? (
            <FormGroup
              advancedSettings={showAdvancedSettings}
              isAdvanced={true}
            >
              <FormLabel>{translate('ListSyncTag')}</FormLabel>
              <FormInputGroup
                {...settings.listSyncTag}
                type={inputTypes.SERIES_TAG}
                name="listSyncTag"
                value={
                  settings.listSyncTag.value === 0
                    ? []
                    : [settings.listSyncTag.value]
                }
                helpText={translate('ListSyncTagHelpText')}
                onChange={handleTagChange}
              />
            </FormGroup>
          ) : null}
        </Form>
      ) : null}
    </FieldSet>
  );
}

export default ImportListOptions;
