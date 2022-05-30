import PropTypes from 'prop-types';
import React, { useEffect, useRef } from 'react';
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
import { inputTypes, kinds } from 'Helpers/Props';
import { authenticationMethodOptions, authenticationRequiredOptions, authenticationRequiredWarning } from 'Settings/General/SecuritySettings';
import styles from './AuthenticationRequiredModalContent.css';

function onModalClose() {
  // No-op
}

function AuthenticationRequiredModalContent(props) {
  const {
    isPopulated,
    error,
    isSaving,
    settings,
    onInputChange,
    onSavePress,
    dispatchFetchStatus
  } = props;

  const {
    authenticationMethod,
    authenticationRequired,
    username,
    password
  } = settings;

  const authenticationEnabled = authenticationMethod && authenticationMethod.value !== 'none';

  const didMount = useRef(false);

  useEffect(() => {
    if (!isSaving && didMount.current) {
      dispatchFetchStatus();
    }

    didMount.current = true;
  }, [isSaving, dispatchFetchStatus]);

  return (
    <ModalContent
      showCloseButton={false}
      onModalClose={onModalClose}
    >
      <ModalHeader>
        Authentication Required
      </ModalHeader>

      <ModalBody>
        <Alert
          className={styles.authRequiredAlert}
          kind={kinds.WARNING}
        >
          {authenticationRequiredWarning}
        </Alert>

        {
          isPopulated && !error ?
            <div>
              <FormGroup>
                <FormLabel>Authentication</FormLabel>

                <FormInputGroup
                  type={inputTypes.SELECT}
                  name="authenticationMethod"
                  values={authenticationMethodOptions}
                  helpText="Require Username and Password to access Sonarr"
                  onChange={onInputChange}
                  {...authenticationMethod}
                />
              </FormGroup>

              {
                authenticationEnabled ?
                  <FormGroup>
                    <FormLabel>Authentication Required</FormLabel>

                    <FormInputGroup
                      type={inputTypes.SELECT}
                      name="authenticationRequired"
                      values={authenticationRequiredOptions}
                      helpText="Change which requests authentication is required for. Do not change unless you understand the risks."
                      onChange={onInputChange}
                      {...authenticationRequired}
                    />
                  </FormGroup> :
                  null
              }

              {
                authenticationEnabled ?
                  <FormGroup>
                    <FormLabel>Username</FormLabel>

                    <FormInputGroup
                      type={inputTypes.TEXT}
                      name="username"
                      onChange={onInputChange}
                      {...username}
                    />
                  </FormGroup> :
                  null
              }

              {
                authenticationEnabled ?
                  <FormGroup>
                    <FormLabel>Password</FormLabel>

                    <FormInputGroup
                      type={inputTypes.PASSWORD}
                      name="password"
                      onChange={onInputChange}
                      {...password}
                    />
                  </FormGroup> :
                  null
              }
            </div> :
            null
        }

        {
          !isPopulated && !error ? <LoadingIndicator /> : null
        }
      </ModalBody>

      <ModalFooter>
        <SpinnerButton
          kind={kinds.PRIMARY}
          isSpinning={isSaving}
          isDisabled={!authenticationEnabled}
          onPress={onSavePress}
        >
          Save
        </SpinnerButton>
      </ModalFooter>
    </ModalContent>
  );
}

AuthenticationRequiredModalContent.propTypes = {
  isPopulated: PropTypes.bool.isRequired,
  error: PropTypes.object,
  isSaving: PropTypes.bool.isRequired,
  saveError: PropTypes.object,
  settings: PropTypes.object.isRequired,
  onInputChange: PropTypes.func.isRequired,
  onSavePress: PropTypes.func.isRequired,
  dispatchFetchStatus: PropTypes.func.isRequired
};

export default AuthenticationRequiredModalContent;
