import React, { useCallback, useEffect } from 'react';
import Alert from 'Components/Alert';
import FormInput from 'Components/Form/FormInput';
import FormInputHelpText from 'Components/Form/FormInputHelpText';
import FormLabel from 'Components/Form/FormLabel';
import FormRow from 'Components/Form/FormRow';
import Link from 'Components/Link/Link';
import SpinnerErrorButton from 'Components/Link/SpinnerErrorButton';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import usePrevious from 'Helpers/Hooks/usePrevious';
import { inputTypes, kinds } from 'Helpers/Props';
import AuthenticationMethodSettings from 'Settings/General/AuthenticationMethodSettings';
import {
  authenticationMethodOptions,
  authenticationRequiredOptions,
} from 'Settings/General/SecuritySettings';
import { useManageGeneralSettings } from 'Settings/General/useGeneralSettings';
import useSystemStatus from 'System/Status/useSystemStatus';
import { InputChanged } from 'typings/inputs';
import translate from 'Utilities/String/translate';

function onModalClose() {
  // No-op
}

export default function AuthenticationRequiredModalContent() {
  const { refetch: refetchStatus } = useSystemStatus();

  const {
    settings,
    isFetched,
    error,
    isSaving,
    saveError,
    saveSettings,
    updateSetting,
  } = useManageGeneralSettings();

  const {
    authenticationMethod,
    authenticationRequired,
    username,
    password,
    passwordConfirmation,
    allowedHosts,
    oidcAuthority,
    oidcClientId,
    oidcClientSecret,
    oidcUserIdentifier,
    oidcScopes,
  } = settings;

  const wasSaving = usePrevious(isSaving);

  const onInputChange = useCallback(
    (change: InputChanged) => {
      // @ts-expect-error input change events aren't typed
      updateSetting(change.name, change.value);
    },
    [updateSetting]
  );

  const authenticationEnabled =
    authenticationMethod && authenticationMethod.value !== 'none';

  useEffect(() => {
    if (isSaving || !wasSaving || saveError) {
      return;
    }

    refetchStatus();
  }, [isSaving, wasSaving, saveError, refetchStatus]);

  const onPress = useCallback(() => {
    saveSettings();
  }, [saveSettings]);

  return (
    <ModalContent showCloseButton={false} onModalClose={onModalClose}>
      <ModalHeader>{translate('AuthenticationRequired')}</ModalHeader>
      <ModalBody>
        <Alert kind={kinds.WARNING}>
          {translate('AuthenticationRequiredWarning')}{' '}
          <Link to="https://wiki.servarr.com/sonarr/faq#forced-authentication">
            {translate('MoreInfo')}
          </Link>
        </Alert>

        {isFetched && !error ? (
          <div>
            <FormRow>
              <FormLabel>{translate('AuthenticationMethod')}</FormLabel>
              <FormInputHelpText
                text={translate('AuthenticationMethodHelpText')}
              />
              <FormInputHelpText
                text={
                  authenticationMethod.value === 'none'
                    ? translate('AuthenticationMethodHelpTextWarning')
                    : undefined
                }
                isWarning={true}
              />
              <FormInput
                type={inputTypes.SELECT}
                name="authenticationMethod"
                values={authenticationMethodOptions}
                onChange={onInputChange}
                {...authenticationMethod}
              />
            </FormRow>

            <FormRow>
              <FormLabel>{translate('AuthenticationRequired')}</FormLabel>
              <FormInputHelpText
                text={translate('AuthenticationRequiredHelpText')}
              />
              <FormInput
                type={inputTypes.SELECT}
                name="authenticationRequired"
                values={authenticationRequiredOptions}
                onChange={onInputChange}
                {...authenticationRequired}
              />
            </FormRow>

            <AuthenticationMethodSettings
              authenticationMethod={authenticationMethod}
              username={username}
              password={password}
              passwordConfirmation={passwordConfirmation}
              oidcAuthority={oidcAuthority}
              oidcClientId={oidcClientId}
              oidcClientSecret={oidcClientSecret}
              oidcUserIdentifier={oidcUserIdentifier}
              oidcScopes={oidcScopes}
              showValidationWarnings={true}
              onInputChange={onInputChange}
            />

            <FormRow>
              <FormLabel>{translate('AllowedHosts')}</FormLabel>
              <FormInputHelpText
                text={translate('AllowedHostsHelpText')}
                link="https://wiki.servarr.com/sonarr/settings#host"
              />
              <FormInputHelpText
                text={translate('RestartRequiredHelpTextWarning')}
                isWarning={true}
              />
              <FormInput
                type={inputTypes.TEXT}
                name="allowedHosts"
                onChange={onInputChange}
                {...allowedHosts}
              />
            </FormRow>
          </div>
        ) : null}

        {!isFetched && !error ? <LoadingIndicator /> : null}
      </ModalBody>
      <ModalFooter>
        <SpinnerErrorButton
          kind={kinds.PRIMARY}
          isSpinning={isSaving}
          isDisabled={!authenticationEnabled}
          error={saveError}
          onPress={onPress}
        >
          {translate('Save')}
        </SpinnerErrorButton>
      </ModalFooter>
    </ModalContent>
  );
}
