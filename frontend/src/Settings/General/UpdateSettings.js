import PropTypes from 'prop-types';
import React from 'react';
import FieldSet from 'Components/FieldSet';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import { inputTypes, sizes } from 'Helpers/Props';
import titleCase from 'Utilities/String/titleCase';
import translate from 'Utilities/String/translate';

const branchValues = [
  'main',
  'develop'
];

function UpdateSettings(props) {
  const {
    advancedSettings,
    settings,
    isWindows,
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
    updateOptions.push({ key: 'builtIn', value: translate('BuiltIn') });
  }

  updateOptions.push({ key: 'script', value: translate('Script') });

  return (
    <FieldSet legend={translate('Updates')}>
      <FormGroup
        advancedSettings={advancedSettings}
        isAdvanced={true}
      >
        <FormLabel>{translate('Branch')}</FormLabel>

        <FormInputGroup
          type={inputTypes.AUTO_COMPLETE}
          name="branch"
          helpText={usingExternalUpdateMechanism ? translate('BranchUpdateMechanism') : translate('BranchUpdate')}
          helpLink="https://wiki.servarr.com/sonarr/settings#updates"
          {...branch}
          values={branchValues}
          onChange={onInputChange}
          readOnly={usingExternalUpdateMechanism}
        />
      </FormGroup>

      {
        isWindows ?
          null :
          <div>
            <FormGroup
              advancedSettings={advancedSettings}
              isAdvanced={true}
              size={sizes.MEDIUM}
            >
              <FormLabel>{translate('Automatic')}</FormLabel>

              <FormInputGroup
                type={inputTypes.CHECK}
                name="updateAutomatically"
                helpText={translate('UpdateAutomaticallyHelpText')}
                helpTextWarning={updateMechanism.value === 'docker' ? translate('AutomaticUpdatesDisabledDocker') : undefined}
                onChange={onInputChange}
                {...updateAutomatically}
              />
            </FormGroup>

            <FormGroup
              advancedSettings={advancedSettings}
              isAdvanced={true}
            >
              <FormLabel>{translate('Mechanism')}</FormLabel>

              <FormInputGroup
                type={inputTypes.SELECT}
                name="updateMechanism"
                values={updateOptions}
                helpText={translate('UpdateMechanismHelpText')}
                helpLink="https://wiki.servarr.com/sonarr/settings#updates"
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
                  <FormLabel>{translate('ScriptPath')}</FormLabel>

                  <FormInputGroup
                    type={inputTypes.TEXT}
                    name="updateScriptPath"
                    helpText={translate('UpdateScriptPathHelpText')}
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
  isWindows: PropTypes.bool.isRequired,
  packageUpdateMechanism: PropTypes.string.isRequired,
  onInputChange: PropTypes.func.isRequired
};

export default UpdateSettings;
