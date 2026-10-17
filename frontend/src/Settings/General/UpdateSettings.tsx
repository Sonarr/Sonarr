import React from 'react';
import FieldSet from 'Components/FieldSet';
import FormInput from 'Components/Form/FormInput';
import FormInputHelpText from 'Components/Form/FormInputHelpText';
import FormLabel from 'Components/Form/FormLabel';
import FormRow from 'Components/Form/FormRow';
import { EnhancedSelectInputValue } from 'Components/Form/Select/EnhancedSelectInput';
import { inputTypes, sizes } from 'Helpers/Props';
import { useShowAdvancedSettings } from 'Settings/advancedSettingsStore';
import { useSystemStatusData } from 'System/Status/useSystemStatus';
import { InputChanged } from 'typings/inputs';
import { PendingSection } from 'typings/pending';
import titleCase from 'Utilities/String/titleCase';
import translate from 'Utilities/String/translate';
import { GeneralSettingsModel } from './useGeneralSettings';

const branchValues = ['main', 'develop'];

interface UpdateSettingsProps {
  branch: PendingSection<GeneralSettingsModel>['branch'];
  updateAutomatically: PendingSection<GeneralSettingsModel>['updateAutomatically'];
  updateMechanism: PendingSection<GeneralSettingsModel>['updateMechanism'];
  updateScriptPath: PendingSection<GeneralSettingsModel>['updateScriptPath'];
  onInputChange: (change: InputChanged) => void;
}

function UpdateSettings({
  branch,
  updateAutomatically,
  updateMechanism,
  updateScriptPath,
  onInputChange,
}: UpdateSettingsProps) {
  const showAdvancedSettings = useShowAdvancedSettings();
  const { packageUpdateMechanism } = useSystemStatusData();

  if (!showAdvancedSettings) {
    return null;
  }

  const usingExternalUpdateMechanism = packageUpdateMechanism !== 'builtIn';

  const updateOptions: EnhancedSelectInputValue<string>[] = [];

  if (usingExternalUpdateMechanism) {
    updateOptions.push({
      key: packageUpdateMechanism,
      value: titleCase(packageUpdateMechanism),
    });
  } else {
    updateOptions.push({ key: 'builtIn', value: translate('BuiltIn') });
  }

  updateOptions.push({ key: 'script', value: translate('Script') });

  return (
    <FieldSet
      legend={translate('Updates')}
      caption={translate('UpdatesCaption')}
    >
      <FormRow advancedSettings={showAdvancedSettings} isAdvanced={true}>
        <FormLabel>{translate('Branch')}</FormLabel>
        <FormInputHelpText
          text={
            usingExternalUpdateMechanism
              ? translate('BranchUpdateMechanism')
              : translate('BranchUpdate')
          }
          link="https://wiki.servarr.com/sonarr/settings#updates"
        />
        <FormInput
          type={inputTypes.AUTO_COMPLETE}
          name="branch"
          {...branch}
          values={branchValues}
          readOnly={usingExternalUpdateMechanism}
          onChange={onInputChange}
        />
      </FormRow>
      <div>
        <FormRow
          advancedSettings={showAdvancedSettings}
          isAdvanced={true}
          size={sizes.MEDIUM}
        >
          <FormLabel>{translate('Automatic')}</FormLabel>
          <FormInputHelpText text={translate('UpdateAutomaticallyHelpText')} />
          <FormInputHelpText
            text={
              updateMechanism.value === 'docker'
                ? translate('AutomaticUpdatesDisabledDocker')
                : undefined
            }
            isWarning={true}
          />
          <FormInput
            type={inputTypes.CHECK}
            name="updateAutomatically"
            onChange={onInputChange}
            {...updateAutomatically}
          />
        </FormRow>

        <FormRow advancedSettings={showAdvancedSettings} isAdvanced={true}>
          <FormLabel>{translate('Mechanism')}</FormLabel>
          <FormInputHelpText
            text={translate('UpdateMechanismHelpText')}
            link="https://wiki.servarr.com/sonarr/settings#updates"
          />
          <FormInput
            type={inputTypes.SELECT}
            name="updateMechanism"
            values={updateOptions}
            onChange={onInputChange}
            {...updateMechanism}
          />
        </FormRow>

        {updateMechanism.value === 'script' ? (
          <FormRow advancedSettings={showAdvancedSettings} isAdvanced={true}>
            <FormLabel>{translate('ScriptPath')}</FormLabel>
            <FormInputHelpText text={translate('UpdateScriptPathHelpText')} />
            <FormInput
              type={inputTypes.TEXT}
              name="updateScriptPath"
              onChange={onInputChange}
              {...updateScriptPath}
            />
          </FormRow>
        ) : null}
      </div>
    </FieldSet>
  );
}

export default UpdateSettings;
