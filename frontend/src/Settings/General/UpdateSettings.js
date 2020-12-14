import PropTypes from 'prop-types';
import React from 'react';
import titleCase from 'Utilities/String/titleCase';
import { inputTypes, sizes } from 'Helpers/Props';
import FieldSet from 'Components/FieldSet';
import FormGroup from 'Components/Form/FormGroup';
import FormLabel from 'Components/Form/FormLabel';
import FormInputGroup from 'Components/Form/FormInputGroup';

const branchValues = [
  'master',
  'develop'
];

function UpdateSettings(props) {
  const {
    advancedSettings,
    settings,
    isMono,
    packageUpdateMechanism,
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

  const usingExternalUpdateMechanism = packageUpdateMechanism !== 'builtIn';

  const updateOptions = [];

  if (usingExternalUpdateMechanism) {
    updateOptions.push({
      key: packageUpdateMechanism,
      value: titleCase(packageUpdateMechanism)
    });
  } else {
    updateOptions.push({ key: 'builtIn', value: 'Built-In' });
  }

  updateOptions.push({ key: 'script', value: 'Script' });

  return (
    <FieldSet legend="Updates">
      <FormGroup
        advancedSettings={advancedSettings}
        isAdvanced={true}
      >
        <FormLabel>Branch</FormLabel>

        <FormInputGroup
          type={inputTypes.AUTO_COMPLETE}
          name="branch"
          helpText={usingExternalUpdateMechanism ? 'Branch used by external update mechanism' : 'Branch to use to update Sonarr'}
          helpLink="https://wiki.servarr.com/Sonarr_Settings#Updates"
          {...branch}
          values={branchValues}
          onChange={onInputChange}
          readOnly={usingExternalUpdateMechanism}
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
                helpLink="https://wiki.servarr.com/Sonarr_Settings#Updates"
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
  packageUpdateMechanism: PropTypes.string.isRequired,
  onInputChange: PropTypes.func.isRequired
};

export default UpdateSettings;
