import PropTypes from 'prop-types';
import React from 'react';
import { inputTypes } from 'Helpers/Props';
import FieldSet from 'Components/FieldSet';
import FormGroup from 'Components/Form/FormGroup';
import FormLabel from 'Components/Form/FormLabel';
import FormInputGroup from 'Components/Form/FormInputGroup';

function ScriptSettings(props) {
  const {
    advancedSettings,
    settings,
    onInputChange
  } = props;

  const {
    scriptFolder
  } = settings;

  if (!advancedSettings) {
    return null;
  }

  return (
    <FieldSet legend="Scripts">
      <FormGroup
        advancedSettings={advancedSettings}
        isAdvanced={true}
      >
        <FormLabel>Folder</FormLabel>

        <FormInputGroup
          type={inputTypes.TEXT}
          name="scriptFolder"
          helpText="Relative paths will be under Sonarr's AppData directory"
          onChange={onInputChange}
          {...scriptFolder}
        />
      </FormGroup>
    </FieldSet>
  );
}

ScriptSettings.propTypes = {
  advancedSettings: PropTypes.bool.isRequired,
  settings: PropTypes.object.isRequired,
  onInputChange: PropTypes.func.isRequired
};

export default ScriptSettings;
