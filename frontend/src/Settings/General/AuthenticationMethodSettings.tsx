import React from 'react';
import { InputChanged } from 'typings/inputs';
import { PendingSection } from 'typings/pending';
import FormsAuthenticationSettings from './FormsAuthenticationSettings';
import OidcAuthenticationSettings from './OidcAuthenticationSettings';
import { GeneralSettingsModel } from './useGeneralSettings';

interface AuthenticationMethodSettingsProps {
  authenticationMethod: PendingSection<GeneralSettingsModel>['authenticationMethod'];
  username: PendingSection<GeneralSettingsModel>['username'];
  password: PendingSection<GeneralSettingsModel>['password'];
  passwordConfirmation: PendingSection<GeneralSettingsModel>['passwordConfirmation'];
  oidcAuthority: PendingSection<GeneralSettingsModel>['oidcAuthority'];
  oidcClientId: PendingSection<GeneralSettingsModel>['oidcClientId'];
  oidcClientSecret: PendingSection<GeneralSettingsModel>['oidcClientSecret'];
  oidcUserIdentifier: PendingSection<GeneralSettingsModel>['oidcUserIdentifier'];
  oidcScopes: PendingSection<GeneralSettingsModel>['oidcScopes'];
  showValidationWarnings?: boolean;
  onInputChange: (change: InputChanged) => void;
}

function AuthenticationMethodSettings({
  authenticationMethod,
  username,
  password,
  passwordConfirmation,
  oidcAuthority,
  oidcClientId,
  oidcClientSecret,
  oidcUserIdentifier,
  oidcScopes,
  showValidationWarnings,
  onInputChange,
}: AuthenticationMethodSettingsProps) {
  switch (authenticationMethod?.value) {
    case 'forms':
      return (
        <FormsAuthenticationSettings
          username={username}
          password={password}
          passwordConfirmation={passwordConfirmation}
          showValidationWarnings={showValidationWarnings}
          onInputChange={onInputChange}
        />
      );

    case 'oidc':
      return (
        <OidcAuthenticationSettings
          oidcAuthority={oidcAuthority}
          oidcClientId={oidcClientId}
          oidcClientSecret={oidcClientSecret}
          oidcUserIdentifier={oidcUserIdentifier}
          oidcScopes={oidcScopes}
          showValidationWarnings={showValidationWarnings}
          onInputChange={onInputChange}
        />
      );

    default:
      return null;
  }
}

export default AuthenticationMethodSettings;
