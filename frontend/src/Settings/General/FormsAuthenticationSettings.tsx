import React from 'react';
import FormInput from 'Components/Form/FormInput';
import FormInputHelpText from 'Components/Form/FormInputHelpText';
import FormLabel from 'Components/Form/FormLabel';
import FormRow from 'Components/Form/FormRow';
import { inputTypes } from 'Helpers/Props';
import { InputChanged } from 'typings/inputs';
import { PendingSection } from 'typings/pending';
import translate from 'Utilities/String/translate';
import { GeneralSettingsModel } from './useGeneralSettings';

interface FormsAuthenticationSettingsProps {
  username: PendingSection<GeneralSettingsModel>['username'];
  password: PendingSection<GeneralSettingsModel>['password'];
  passwordConfirmation: PendingSection<GeneralSettingsModel>['passwordConfirmation'];
  showValidationWarnings?: boolean;
  onInputChange: (change: InputChanged) => void;
}

function FormsAuthenticationSettings({
  username,
  password,
  passwordConfirmation,
  showValidationWarnings = false,
  onInputChange,
}: FormsAuthenticationSettingsProps) {
  return (
    <>
      <FormRow>
        <FormLabel>{translate('Username')}</FormLabel>
        <FormInputHelpText
          text={
            showValidationWarnings && !username?.value
              ? translate('AuthenticationRequiredUsernameHelpTextWarning')
              : undefined
          }
          isWarning={true}
        />
        <FormInput
          type={inputTypes.TEXT}
          name="username"
          onChange={onInputChange}
          {...username}
        />
      </FormRow>

      <FormRow>
        <FormLabel>{translate('Password')}</FormLabel>
        <FormInputHelpText
          text={
            showValidationWarnings && !password?.value
              ? translate('AuthenticationRequiredPasswordHelpTextWarning')
              : undefined
          }
          isWarning={true}
        />
        <FormInput
          type={inputTypes.PASSWORD}
          name="password"
          onChange={onInputChange}
          {...password}
        />
      </FormRow>

      <FormRow>
        <FormLabel>{translate('PasswordConfirmation')}</FormLabel>
        <FormInputHelpText
          text={
            showValidationWarnings && !passwordConfirmation?.value
              ? translate(
                  'AuthenticationRequiredPasswordConfirmationHelpTextWarning'
                )
              : undefined
          }
          isWarning={true}
        />
        <FormInput
          type={inputTypes.PASSWORD}
          name="passwordConfirmation"
          onChange={onInputChange}
          {...passwordConfirmation}
        />
      </FormRow>
    </>
  );
}

export default FormsAuthenticationSettings;
