import React, { useCallback, useEffect } from 'react';
import Alert from 'Components/Alert';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
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
import styles from './AuthenticationRequiredModalContent.css';

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
        <Alert className={styles.authRequiredAlert} kind={kinds.WARNING}>
          {translate('AuthenticationRequiredWarning')}
        </Alert>

        {isFetched && !error ? (
          <div>
            <FormGroup>
              <FormLabel>{translate('AuthenticationMethod')}</FormLabel>

              <FormInputGroup
                type={inputTypes.SELECT}
                name="authenticationMethod"
                values={authenticationMethodOptions}
                helpText={translate('AuthenticationMethodHelpText')}
                helpTextWarning={
                  authenticationMethod.value === 'none'
                    ? translate('AuthenticationMethodHelpTextWarning')
                    : undefined
                }
                helpLink="https://wiki.servarr.com/sonarr/faq#forced-authentication"
                onChange={onInputChange}
                {...authenticationMethod}
              />
            </FormGroup>

            <FormGroup>
              <FormLabel>{translate('AuthenticationRequired')}</FormLabel>

              <FormInputGroup
                type={inputTypes.SELECT}
                name="authenticationRequired"
                values={authenticationRequiredOptions}
                helpText={translate('AuthenticationRequiredHelpText')}
                onChange={onInputChange}
                {...authenticationRequired}
              />
            </FormGroup>

            <FormGroup>
              <FormLabel>{translate('Username')}</FormLabel>

              <FormInputGroup
                type={inputTypes.TEXT}
                name="username"
                helpTextWarning={
                  username?.value
                    ? undefined
                    : translate('AuthenticationRequiredUsernameHelpTextWarning')
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
                  password?.value
                    ? undefined
                    : translate('AuthenticationRequiredPasswordHelpTextWarning')
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
                  passwordConfirmation?.value
                    ? undefined
                    : translate(
                        'AuthenticationRequiredPasswordConfirmationHelpTextWarning'
                      )
                }
                onChange={onInputChange}
                {...passwordConfirmation}
              />
            </FormGroup>
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
