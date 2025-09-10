import React, { useCallback, useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
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
import { clearPendingChanges } from 'Store/Actions/baseActions';
import {
  fetchGeneralSettings,
  saveGeneralSettings,
  setGeneralSettingsValue,
} from 'Store/Actions/settingsActions';
import { fetchStatus } from 'Store/Actions/systemActions';
import createSettingsSectionSelector from 'Store/Selectors/createSettingsSectionSelector';
import { InputChanged } from 'typings/inputs';
import translate from 'Utilities/String/translate';
import styles from './AuthenticationRequiredModalContent.css';

const SECTION = 'general';

const selector = createSettingsSectionSelector(SECTION);

function onModalClose() {
  // No-op
}

export default function AuthenticationRequiredModalContent() {
  const { isPopulated, error, isSaving, settings } = useSelector(selector);
  const dispatch = useDispatch();

  const {
    authenticationMethod,
    authenticationRequired,
    username,
    password,
    passwordConfirmation,
  } = settings;

  const wasSaving = usePrevious(isSaving);

  useEffect(() => {
    dispatch(fetchGeneralSettings());

    return () => {
      dispatch(clearPendingChanges({ section: `settings.${SECTION}` }));
    };
  }, [dispatch]);

  const onInputChange = useCallback(
    (args: InputChanged) => {
      // @ts-expect-error Actions aren't typed
      dispatch(setGeneralSettingsValue(args));
    },
    [dispatch]
  );

  const authenticationEnabled =
    authenticationMethod && authenticationMethod.value !== 'none';

  useEffect(() => {
    if (isSaving || !wasSaving) {
      return;
    }

    dispatch(fetchStatus());
  }, [isSaving, wasSaving, dispatch]);

  const onPress = useCallback(() => {
    dispatch(saveGeneralSettings());
  }, [dispatch]);

  return (
    <ModalContent showCloseButton={false} onModalClose={onModalClose}>
      <ModalHeader>{translate('AuthenticationRequired')}</ModalHeader>

      <ModalBody>
        <Alert className={styles.authRequiredAlert} kind={kinds.WARNING}>
          {translate('AuthenticationRequiredWarning')}
        </Alert>

        {isPopulated && !error ? (
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

        {!isPopulated && !error ? <LoadingIndicator /> : null}
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
