import React, { useCallback, useEffect } from 'react';
import Alert from 'Components/Alert';
import FormInput from 'Components/Form/FormInput';
import FormInputHelpText from 'Components/Form/FormInputHelpText';
import FormLabel from 'Components/Form/FormLabel';
import FormRow from 'Components/Form/FormRow';
import Link from 'Components/Link/Link';
import SpinnerButton from 'Components/Link/SpinnerButton';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import usePrevious from 'Helpers/Hooks/usePrevious';
import { inputTypes, kinds } from 'Helpers/Props';
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

  const { settings, isFetched, error, isSaving, saveSettings, updateSetting } =
    useManageGeneralSettings();

  const {
    authenticationMethod,
    authenticationRequired,
    username,
    password,
    passwordConfirmation,
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
    if (isSaving || !wasSaving) {
      return;
    }

    refetchStatus();
  }, [isSaving, wasSaving, refetchStatus]);

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

            <FormRow>
              <FormLabel>{translate('Username')}</FormLabel>
              <FormInputHelpText
                text={
                  username?.value
                    ? undefined
                    : translate('AuthenticationRequiredUsernameHelpTextWarning')
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
                  password?.value
                    ? undefined
                    : translate('AuthenticationRequiredPasswordHelpTextWarning')
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
                  passwordConfirmation?.value
                    ? undefined
                    : translate(
                        'AuthenticationRequiredPasswordConfirmationHelpTextWarning'
                      )
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
          </div>
        ) : null}

        {!isFetched && !error ? <LoadingIndicator /> : null}
      </ModalBody>
      <ModalFooter>
        <SpinnerButton
          kind={kinds.PRIMARY}
          isSpinning={isSaving}
          isDisabled={!authenticationEnabled}
          onPress={onPress}
        >
          {translate('Save')}
        </SpinnerButton>
      </ModalFooter>
    </ModalContent>
  );
}
