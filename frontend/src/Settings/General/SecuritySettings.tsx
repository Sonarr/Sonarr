import React, { FocusEvent, useCallback, useState } from 'react';
import { useDispatch } from 'react-redux';
import * as commandNames from 'Commands/commandNames';
import FieldSet from 'Components/FieldSet';
import FormGroup from 'Components/Form/FormGroup';
import FormInputButton from 'Components/Form/FormInputButton';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import Icon from 'Components/Icon';
import ClipboardButton from 'Components/Link/ClipboardButton';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import { icons, inputTypes, kinds } from 'Helpers/Props';
import { executeCommand } from 'Store/Actions/commandActions';
import { InputChanged } from 'typings/inputs';
import { PendingSection } from 'typings/pending';
import General from 'typings/Settings/General';
import translate from 'Utilities/String/translate';

export const authenticationMethodOptions = [
  {
    key: 'none',
    get value() {
      return translate('None');
    },
    isDisabled: true,
  },
  {
    key: 'external',
    get value() {
      return translate('External');
    },
    isHidden: true,
  },
  {
    key: 'basic',
    get value() {
      return translate('AuthBasic');
    },
  },
  {
    key: 'forms',
    get value() {
      return translate('AuthForm');
    },
  },
];

export const authenticationRequiredOptions = [
  {
    key: 'enabled',
    get value() {
      return translate('Enabled');
    },
  },
  {
    key: 'disabledForLocalAddresses',
    get value() {
      return translate('DisabledForLocalAddresses');
    },
  },
];

const certificateValidationOptions = [
  {
    key: 'enabled',
    get value() {
      return translate('Enabled');
    },
  },
  {
    key: 'disabledForLocalAddresses',
    get value() {
      return translate('DisabledForLocalAddresses');
    },
  },
  {
    key: 'disabled',
    get value() {
      return translate('Disabled');
    },
  },
];

interface SecuritySettingsProps {
  authenticationMethod: PendingSection<General>['authenticationMethod'];
  authenticationRequired: PendingSection<General>['authenticationRequired'];
  username: PendingSection<General>['username'];
  password: PendingSection<General>['password'];
  passwordConfirmation: PendingSection<General>['passwordConfirmation'];
  apiKey: PendingSection<General>['apiKey'];
  certificateValidation: PendingSection<General>['certificateValidation'];
  isResettingApiKey: boolean;
  onInputChange: (change: InputChanged) => void;
}

function SecuritySettings({
  authenticationMethod,
  authenticationRequired,
  username,
  password,
  passwordConfirmation,
  apiKey,
  certificateValidation,
  isResettingApiKey,
  onInputChange,
}: SecuritySettingsProps) {
  const dispatch = useDispatch();

  const [isConfirmApiKeyResetModalOpen, setIsConfirmApiKeyResetModalOpen] =
    useState(false);

  const handleApikeyFocus = useCallback(
    (event: FocusEvent<HTMLInputElement, Element>) => {
      event.target.select();
    },
    []
  );

  const handleResetApiKeyPress = useCallback(() => {
    setIsConfirmApiKeyResetModalOpen(true);
  }, []);

  const handleConfirmResetApiKey = useCallback(() => {
    setIsConfirmApiKeyResetModalOpen(false);

    dispatch(executeCommand({ name: commandNames.RESET_API_KEY }));
  }, [dispatch]);

  const handleCloseResetApiKeyModal = useCallback(() => {
    setIsConfirmApiKeyResetModalOpen(false);
  }, []);

  // createCommandExecutingSelector(commandNames.RESET_API_KEY),

  const authenticationEnabled =
    authenticationMethod && authenticationMethod.value !== 'none';

  return (
    <FieldSet legend={translate('Security')}>
      <FormGroup>
        <FormLabel>{translate('Authentication')}</FormLabel>

        <FormInputGroup
          type={inputTypes.SELECT}
          name="authenticationMethod"
          values={authenticationMethodOptions}
          helpText={translate('AuthenticationMethodHelpText')}
          helpTextWarning={translate('AuthenticationRequiredWarning')}
          onChange={onInputChange}
          {...authenticationMethod}
        />
      </FormGroup>

      {authenticationEnabled ? (
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
      ) : null}

      {authenticationEnabled ? (
        <FormGroup>
          <FormLabel>{translate('Username')}</FormLabel>

          <FormInputGroup
            type={inputTypes.TEXT}
            name="username"
            onChange={onInputChange}
            {...username}
          />
        </FormGroup>
      ) : null}

      {authenticationEnabled ? (
        <FormGroup>
          <FormLabel>{translate('Password')}</FormLabel>

          <FormInputGroup
            type={inputTypes.PASSWORD}
            name="password"
            onChange={onInputChange}
            {...password}
          />
        </FormGroup>
      ) : null}

      {authenticationEnabled ? (
        <FormGroup>
          <FormLabel>{translate('PasswordConfirmation')}</FormLabel>

          <FormInputGroup
            type={inputTypes.PASSWORD}
            name="passwordConfirmation"
            onChange={onInputChange}
            {...passwordConfirmation}
          />
        </FormGroup>
      ) : null}

      <FormGroup>
        <FormLabel>{translate('ApiKey')}</FormLabel>

        <FormInputGroup
          type={inputTypes.TEXT}
          name="apiKey"
          readOnly={true}
          helpTextWarning={translate('RestartRequiredHelpTextWarning')}
          buttons={[
            <ClipboardButton
              key="copy"
              value={apiKey.value}
              kind={kinds.DEFAULT}
            />,

            <FormInputButton
              key="reset"
              kind={kinds.DANGER}
              onPress={handleResetApiKeyPress}
            >
              <Icon name={icons.REFRESH} isSpinning={isResettingApiKey} />
            </FormInputButton>,
          ]}
          onChange={onInputChange}
          onFocus={handleApikeyFocus}
          {...apiKey}
        />
      </FormGroup>

      <FormGroup>
        <FormLabel>{translate('CertificateValidation')}</FormLabel>

        <FormInputGroup
          type={inputTypes.SELECT}
          name="certificateValidation"
          values={certificateValidationOptions}
          helpText={translate('CertificateValidationHelpText')}
          onChange={onInputChange}
          {...certificateValidation}
        />
      </FormGroup>

      <ConfirmModal
        isOpen={isConfirmApiKeyResetModalOpen}
        kind={kinds.DANGER}
        title={translate('ResetAPIKey')}
        message={translate('ResetAPIKeyMessageText')}
        confirmLabel={translate('Reset')}
        onConfirm={handleConfirmResetApiKey}
        onCancel={handleCloseResetApiKeyModal}
      />
    </FieldSet>
  );
}

export default SecuritySettings;
