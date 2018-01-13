import PropTypes from 'prop-types';
import React from 'react';
import { inputTypes, sizes } from 'Helpers/Props';
import FieldSet from 'Components/FieldSet';
import FormGroup from 'Components/Form/FormGroup';
import FormLabel from 'Components/Form/FormLabel';
import FormInputGroup from 'Components/Form/FormInputGroup';

function UpdateSettings(props) {
  const {
    advancedSettings,
    settings,
    isMono,
    onInputChange
  } = props;

  const {
    branch,
    updateAutomatically,
    updateMechanism,
    updateScriptPath
  } = settings;

  if (!advancedSettings) {
    return null;
  }

  const updateOptions = [
    { key: 'builtIn', value: 'Built-In' },
    { key: 'script', value: 'Script' }
  ];

  return (
    <FieldSet legend="Updates">
      <FormGroup
        advancedSettings={advancedSettings}
        isAdvanced={true}
      >
        <FormLabel>Branch</FormLabel>

        <FormInputGroup
          type={inputTypes.TEXT}
          name="branch"
          helpText="Branch to use to update Sonarr"
          helpLink="https://github.com/Sonarr/Sonarr/wiki/Release-Branches"
          onChange={onInputChange}
          {...branch}
        />
      </FormGroup>

      {
        isMono &&
        <div>
          <FormGroup
            advancedSettings={advancedSettings}
            isAdvanced={true}
            size={sizes.MEDIUM}
          >
            <FormLabel>Automatic</FormLabel>

            <FormInputGroup
              type={inputTypes.CHECK}
              name="updateAutomatically"
              helpText="Automatically download and install updates. You will still be able to install from System: Updates"
              onChange={onInputChange}
              {...updateAutomatically}
            />
          </FormGroup>

          <FormGroup
            advancedSettings={advancedSettings}
            isAdvanced={true}
          >
            <FormLabel>Mechanism</FormLabel>

            <FormInputGroup
              type={inputTypes.SELECT}
              name="updateMechanism"
              values={updateOptions}
              helpText="Use Sonarr's built-in updater or a script"
              helpLink="https://github.com/Sonarr/Sonarr/wiki/Updating"
              onChange={onInputChange}
              {...updateMechanism}
            />
          </FormGroup>

          {
            updateMechanism.value === 'script' &&
            <FormGroup
              advancedSettings={advancedSettings}
              isAdvanced={true}
            >
              <FormLabel>Script Path</FormLabel>

              <FormInputGroup
                type={inputTypes.TEXT}
                name="updateScriptPath"
                helpText="Path to a custom script that takes an extracted update package and handle the remainder of the update process"
                onChange={onInputChange}
                {...updateScriptPath}
              />
            </FormGroup>
          }
        </div>
      }
    </FieldSet>
  );
}

UpdateSettings.propTypes = {
  advancedSettings: PropTypes.bool.isRequired,
  settings: PropTypes.object.isRequired,
  isMono: PropTypes.bool.isRequired,
  onInputChange: PropTypes.func.isRequired
};

export default UpdateSettings;
