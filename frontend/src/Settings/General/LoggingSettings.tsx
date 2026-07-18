import React from 'react';
import FieldSet from 'Components/FieldSet';
import FormInput from 'Components/Form/FormInput';
import FormInputHelpText from 'Components/Form/FormInputHelpText';
import FormLabel from 'Components/Form/FormLabel';
import FormRow from 'Components/Form/FormRow';
import { EnhancedSelectInputValue } from 'Components/Form/Select/EnhancedSelectInput';
import { inputTypes } from 'Helpers/Props';
import { useShowAdvancedSettings } from 'Settings/advancedSettingsStore';
import { InputChanged } from 'typings/inputs';
import { PendingSection } from 'typings/pending';
import translate from 'Utilities/String/translate';
import { GeneralSettingsModel } from './useGeneralSettings';

const logLevelOptions: EnhancedSelectInputValue<string>[] = [
  {
    key: 'info',
    get value() {
      return translate('Info');
    },
  },
  {
    key: 'debug',
    get value() {
      return translate('Debug');
    },
  },
  {
    key: 'trace',
    get value() {
      return translate('Trace');
    },
  },
];

interface LoggingSettingsProps {
  logLevel: PendingSection<GeneralSettingsModel>['logLevel'];
  logSizeLimit: PendingSection<GeneralSettingsModel>['logSizeLimit'];
  onInputChange: (change: InputChanged) => void;
}

function LoggingSettings({
  logLevel,
  logSizeLimit,
  onInputChange,
}: LoggingSettingsProps) {
  const showAdvancedSettings = useShowAdvancedSettings();

  return (
    <FieldSet
      legend={translate('Logging')}
      caption={translate('LoggingCaption')}
    >
      <FormRow>
        <FormLabel>{translate('LogLevel')}</FormLabel>
        <FormInputHelpText
          text={
            logLevel.value === 'trace'
              ? translate('LogLevelTraceHelpTextWarning')
              : undefined
          }
          isWarning={true}
        />
        <FormInput
          type={inputTypes.SELECT}
          name="logLevel"
          values={logLevelOptions}
          onChange={onInputChange}
          {...logLevel}
        />
      </FormRow>
      <FormRow advancedSettings={showAdvancedSettings} isAdvanced={true}>
        <FormLabel>{translate('LogSizeLimit')}</FormLabel>
        <FormInputHelpText text={translate('LogSizeLimitHelpText')} />
        <FormInput
          type={inputTypes.NUMBER}
          name="logSizeLimit"
          min={1}
          max={10}
          unit="MB"
          onChange={onInputChange}
          {...logSizeLimit}
        />
      </FormRow>
    </FieldSet>
  );
}

export default LoggingSettings;
