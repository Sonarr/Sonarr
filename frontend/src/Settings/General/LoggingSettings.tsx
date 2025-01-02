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

const logLevelOptions = [
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
  logLevel: PendingSection<General>['logLevel'];
  logSizeLimit: PendingSection<General>['logSizeLimit'];
  onInputChange: (change: InputChanged) => void;
}

function LoggingSettings({
  logLevel,
  logSizeLimit,
  onInputChange,
}: LoggingSettingsProps) {
  const showAdvancedSettings = useShowAdvancedSettings();

  return (
    <FieldSet legend={translate('Logging')}>
      <FormGroup>
        <FormLabel>{translate('LogLevel')}</FormLabel>

        <FormInputGroup
          type={inputTypes.SELECT}
          name="logLevel"
          values={logLevelOptions}
          helpTextWarning={
            logLevel.value === 'trace'
              ? translate('LogLevelTraceHelpTextWarning')
              : undefined
          }
          onChange={onInputChange}
          {...logLevel}
        />
      </FormGroup>

      <FormGroup advancedSettings={showAdvancedSettings} isAdvanced={true}>
        <FormLabel>{translate('LogSizeLimit')}</FormLabel>

        <FormInputGroup
          type={inputTypes.NUMBER}
          name="logSizeLimit"
          min={1}
          max={10}
          unit="MB"
          helpText={translate('LogSizeLimitHelpText')}
          onChange={onInputChange}
          {...logSizeLimit}
        />
      </FormGroup>
    </FieldSet>
  );
}

export default LoggingSettings;
