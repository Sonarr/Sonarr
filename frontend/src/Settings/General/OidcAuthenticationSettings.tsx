import React from 'react';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import { inputTypes } from 'Helpers/Props';
import { InputChanged } from 'typings/inputs';
import { PendingSection } from 'typings/pending';
import translate from 'Utilities/String/translate';
import { GeneralSettingsModel } from './useGeneralSettings';

interface OidcAuthenticationSettingsProps {
  oidcAuthority: PendingSection<GeneralSettingsModel>['oidcAuthority'];
  oidcClientId: PendingSection<GeneralSettingsModel>['oidcClientId'];
  oidcClientSecret: PendingSection<GeneralSettingsModel>['oidcClientSecret'];
  oidcUserIdentifier: PendingSection<GeneralSettingsModel>['oidcUserIdentifier'];
  oidcScopes: PendingSection<GeneralSettingsModel>['oidcScopes'];
  showValidationWarnings?: boolean;
  onInputChange: (change: InputChanged) => void;
}

function OidcAuthenticationSettings({
  oidcAuthority,
  oidcClientId,
  oidcClientSecret,
  oidcUserIdentifier,
  oidcScopes,
  showValidationWarnings = false,
  onInputChange,
}: OidcAuthenticationSettingsProps) {
  return (
    <>
      <FormGroup>
        <FormLabel>{translate('OidcAuthority')}</FormLabel>

        <FormInputGroup
          type={inputTypes.TEXT}
          name="oidcAuthority"
          helpText={translate('OidcAuthorityHelpText')}
          helpTextWarning={
            showValidationWarnings && !oidcAuthority?.value
              ? translate('AuthenticationRequiredOidcAuthorityHelpTextWarning')
              : undefined
          }
          onChange={onInputChange}
          {...oidcAuthority}
        />
      </FormGroup>

      <FormGroup>
        <FormLabel>{translate('OidcClientId')}</FormLabel>

        <FormInputGroup
          type={inputTypes.TEXT}
          name="oidcClientId"
          helpText={translate('OidcClientIdHelpText')}
          helpTextWarning={
            showValidationWarnings && !oidcClientId?.value
              ? translate('AuthenticationRequiredOidcClientIdHelpTextWarning')
              : undefined
          }
          onChange={onInputChange}
          {...oidcClientId}
        />
      </FormGroup>

      <FormGroup>
        <FormLabel>{translate('OidcClientSecret')}</FormLabel>

        <FormInputGroup
          type={inputTypes.PASSWORD}
          name="oidcClientSecret"
          helpText={translate('OidcClientSecretHelpText')}
          helpTextWarning={
            showValidationWarnings && !oidcClientSecret?.value
              ? translate(
                  'AuthenticationRequiredOidcClientSecretHelpTextWarning'
                )
              : undefined
          }
          onChange={onInputChange}
          {...oidcClientSecret}
        />
      </FormGroup>

      <FormGroup>
        <FormLabel>{translate('OidcUserIdentifier')}</FormLabel>

        <FormInputGroup
          type={inputTypes.TEXT}
          name="oidcUserIdentifier"
          helpText={translate('OidcUserIdentifierHelpText')}
          helpTextWarning={
            showValidationWarnings && !oidcUserIdentifier?.value
              ? translate(
                  'AuthenticationRequiredOidcUserIdentifierHelpTextWarning'
                )
              : undefined
          }
          onChange={onInputChange}
          {...oidcUserIdentifier}
        />
      </FormGroup>

      <FormGroup>
        <FormLabel>{translate('OidcScopes')}</FormLabel>

        <FormInputGroup
          type={inputTypes.TEXT}
          name="oidcScopes"
          helpText={translate('OidcScopesHelpText')}
          onChange={onInputChange}
          {...oidcScopes}
        />
      </FormGroup>
    </>
  );
}

export default OidcAuthenticationSettings;
