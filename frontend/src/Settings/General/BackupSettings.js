import PropTypes from 'prop-types';
import React from 'react';
import { inputTypes } from 'Helpers/Props';
import FieldSet from 'Components/FieldSet';
import FormGroup from 'Components/Form/FormGroup';
import FormLabel from 'Components/Form/FormLabel';
import FormInputGroup from 'Components/Form/FormInputGroup';

function BackupSettings(props) {
  const {
    advancedSettings,
    settings,
    onInputChange
  } = props;

  const {
    backupFolder,
    backupInterval,
    backupRetention
  } = settings;

  if (!advancedSettings) {
    return null;
  }

  return (
    <FieldSet legend="Backups">
      <FormGroup
        advancedSettings={advancedSettings}
        isAdvanced={true}
      >
        <FormLabel>Folder</FormLabel>

        <FormInputGroup
          type={inputTypes.TEXT}
          name="backupFolder"
          helpText="Relative paths will be under Sonarr's AppData directory"
          onChange={onInputChange}
          {...backupFolder}
        />
      </FormGroup>

      <FormGroup
        advancedSettings={advancedSettings}
        isAdvanced={true}
      >
        <FormLabel>Interval</FormLabel>

        <FormInputGroup
          type={inputTypes.NUMBER}
          name="backupInterval"
          helpText="Interval in days"
          onChange={onInputChange}
          {...backupInterval}
        />
      </FormGroup>

      <FormGroup
        advancedSettings={advancedSettings}
        isAdvanced={true}
      >
        <FormLabel>Retention</FormLabel>

        <FormInputGroup
          type={inputTypes.NUMBER}
          name="backupRetention"
          helpText="Retention in days. Automatic backups older the retention will be cleaned up automatically"
          onChange={onInputChange}
          {...backupRetention}
        />
      </FormGroup>
    </FieldSet>
  );
}

BackupSettings.propTypes = {
  advancedSettings: PropTypes.bool.isRequired,
  settings: PropTypes.object.isRequired,
  onInputChange: PropTypes.func.isRequired
};

export default BackupSettings;
