import React from 'react';
import FieldSet from 'Components/FieldSet';
import FormInput from 'Components/Form/FormInput';
import FormInputHelpText from 'Components/Form/FormInputHelpText';
import FormLabel from 'Components/Form/FormLabel';
import FormRow from 'Components/Form/FormRow';
import { inputTypes, sizes } from 'Helpers/Props';
import { InputChanged } from 'typings/inputs';
import { PendingSection } from 'typings/pending';
import translate from 'Utilities/String/translate';
import { GeneralSettingsModel } from './useGeneralSettings';

interface AnalyticSettingsProps {
  analyticsEnabled: PendingSection<GeneralSettingsModel>['analyticsEnabled'];
  onInputChange: (change: InputChanged) => void;
}

function AnalyticSettings({
  analyticsEnabled,
  onInputChange,
}: AnalyticSettingsProps) {
  return (
    <FieldSet
      legend={translate('Analytics')}
      caption={translate('AnalyticsCaption')}
    >
      <FormRow size={sizes.MEDIUM}>
        <FormLabel>{translate('SendAnonymousUsageData')}</FormLabel>
        <FormInputHelpText text={translate('AnalyticsEnabledHelpText')} />
        <FormInputHelpText
          text={translate('RestartRequiredHelpTextWarning')}
          isWarning={true}
        />
        <FormInput
          type={inputTypes.CHECK}
          name="analyticsEnabled"
          onChange={onInputChange}
          {...analyticsEnabled}
        />
      </FormRow>
    </FieldSet>
  );
}

export default AnalyticSettings;
