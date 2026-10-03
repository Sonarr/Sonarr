import React from 'react';
import FieldSet from 'Components/FieldSet';
import FormInput from 'Components/Form/FormInput';
import FormInputHelpText from 'Components/Form/FormInputHelpText';
import FormLabel from 'Components/Form/FormLabel';
import FormRow from 'Components/Form/FormRow';
import { inputTypes } from 'Helpers/Props';
import { useShowAdvancedSettings } from 'Settings/advancedSettingsStore';
import { InputChanged } from 'typings/inputs';
import { PendingSection } from 'typings/pending';
import translate from 'Utilities/String/translate';
import { GeneralSettingsModel } from './useGeneralSettings';

interface BackupSettingsProps {
  backupFolder: PendingSection<GeneralSettingsModel>['backupFolder'];
  backupInterval: PendingSection<GeneralSettingsModel>['backupInterval'];
  backupRetention: PendingSection<GeneralSettingsModel>['backupRetention'];
  onInputChange: (change: InputChanged) => void;
}

function BackupSettings({
  backupFolder,
  backupInterval,
  backupRetention,
  onInputChange,
}: BackupSettingsProps) {
  const showAdvancedSettings = useShowAdvancedSettings();

  if (!showAdvancedSettings) {
    return null;
  }

  return (
    <FieldSet
      legend={translate('Backups')}
      caption={translate('BackupsCaption')}
    >
      <FormRow advancedSettings={showAdvancedSettings} isAdvanced={true}>
        <FormLabel>{translate('Folder')}</FormLabel>
        <FormInputHelpText text={translate('BackupFolderHelpText')} />
        <FormInput
          type={inputTypes.PATH}
          name="backupFolder"
          includeFiles={false}
          onChange={onInputChange}
          {...backupFolder}
        />
      </FormRow>
      <FormRow advancedSettings={showAdvancedSettings} isAdvanced={true}>
        <FormLabel>{translate('Interval')}</FormLabel>
        <FormInputHelpText text={translate('BackupIntervalHelpText')} />
        <FormInput
          type={inputTypes.NUMBER}
          name="backupInterval"
          unit="days"
          onChange={onInputChange}
          {...backupInterval}
        />
      </FormRow>
      <FormRow advancedSettings={showAdvancedSettings} isAdvanced={true}>
        <FormLabel>{translate('Retention')}</FormLabel>
        <FormInputHelpText text={translate('BackupRetentionHelpText')} />
        <FormInput
          type={inputTypes.NUMBER}
          name="backupRetention"
          unit="days"
          onChange={onInputChange}
          {...backupRetention}
        />
      </FormRow>
    </FieldSet>
  );
}

export default BackupSettings;
