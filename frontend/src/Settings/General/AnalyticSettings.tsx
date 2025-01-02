import React from 'react';
import FieldSet from 'Components/FieldSet';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import { inputTypes, sizes } from 'Helpers/Props';
import { InputChanged } from 'typings/inputs';
import { PendingSection } from 'typings/pending';
import General from 'typings/Settings/General';
import translate from 'Utilities/String/translate';

interface AnalyticSettingsProps {
  analyticsEnabled: PendingSection<General>['analyticsEnabled'];
  onInputChange: (change: InputChanged) => void;
}

function AnalyticSettings({
  analyticsEnabled,
  onInputChange,
}: AnalyticSettingsProps) {
  return (
    <FieldSet legend={translate('Analytics')}>
      <FormGroup size={sizes.MEDIUM}>
        <FormLabel>{translate('SendAnonymousUsageData')}</FormLabel>

        <FormInputGroup
          type={inputTypes.CHECK}
          name="analyticsEnabled"
          helpText={translate('AnalyticsEnabledHelpText')}
          helpTextWarning={translate('RestartRequiredHelpTextWarning')}
          onChange={onInputChange}
          {...analyticsEnabled}
        />
      </FormGroup>
    </FieldSet>
  );
}

export default AnalyticSettings;
