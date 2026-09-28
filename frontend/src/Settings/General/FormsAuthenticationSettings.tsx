import React from 'react';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
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
      <FormGroup>
        <FormLabel>{translate('Username')}</FormLabel>

        <FormInputGroup
          type={inputTypes.TEXT}
          name="username"
          helpTextWarning={
            showValidationWarnings && !username?.value
              ? translate('AuthenticationRequiredUsernameHelpTextWarning')
              : undefined
          }
          onChange={onInputChange}
          {...username}
        />
      </FormGroup>

      <FormGroup>
        <FormLabel>{translate('Password')}</FormLabel>

        <FormInputGroup
          type={inputTypes.PASSWORD}
          name="password"
          helpTextWarning={
            showValidationWarnings && !password?.value
              ? translate('AuthenticationRequiredPasswordHelpTextWarning')
              : undefined
          }
          onChange={onInputChange}
          {...password}
        />
      </FormGroup>

      <FormGroup>
        <FormLabel>{translate('PasswordConfirmation')}</FormLabel>

        <FormInputGroup
          type={inputTypes.PASSWORD}
          name="passwordConfirmation"
          helpTextWarning={
            showValidationWarnings && !passwordConfirmation?.value
              ? translate(
                  'AuthenticationRequiredPasswordConfirmationHelpTextWarning'
                )
              : undefined
          }
          onChange={onInputChange}
          {...passwordConfirmation}
        />
      </FormGroup>
    </>
  );
}

export default FormsAuthenticationSettings;
