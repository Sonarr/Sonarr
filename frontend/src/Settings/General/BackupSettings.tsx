import React from 'react';
import FieldSet from 'Components/FieldSet';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import useShowAdvancedSettings from 'Helpers/Hooks/useShowAdvancedSettings';
import { inputTypes } from 'Helpers/Props';
import { InputChanged } from 'typings/inputs';
import { PendingSection } from 'typings/pending';
import General from 'typings/Settings/General';
import translate from 'Utilities/String/translate';

interface BackupSettingsProps {
  backupFolder: PendingSection<General>['backupFolder'];
  backupInterval: PendingSection<General>['backupInterval'];
  backupRetention: PendingSection<General>['backupRetention'];
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
    <FieldSet legend={translate('Backups')}>
      <FormGroup advancedSettings={showAdvancedSettings} isAdvanced={true}>
        <FormLabel>{translate('Folder')}</FormLabel>

        <FormInputGroup
          type={inputTypes.PATH}
          name="backupFolder"
          helpText={translate('BackupFolderHelpText')}
          onChange={onInputChange}
          {...backupFolder}
        />
      </FormGroup>

      <FormGroup advancedSettings={showAdvancedSettings} isAdvanced={true}>
        <FormLabel>{translate('Interval')}</FormLabel>

        <FormInputGroup
          type={inputTypes.NUMBER}
          name="backupInterval"
          unit="days"
          helpText={translate('BackupIntervalHelpText')}
          onChange={onInputChange}
          {...backupInterval}
        />
      </FormGroup>

      <FormGroup advancedSettings={showAdvancedSettings} isAdvanced={true}>
        <FormLabel>{translate('Retention')}</FormLabel>

        <FormInputGroup
          type={inputTypes.NUMBER}
          name="backupRetention"
          unit="days"
          helpText={translate('BackupRetentionHelpText')}
          onChange={onInputChange}
          {...backupRetention}
        />
      </FormGroup>
    </FieldSet>
  );
}

export default BackupSettings;
