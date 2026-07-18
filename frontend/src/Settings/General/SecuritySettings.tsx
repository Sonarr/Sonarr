import React, { FocusEvent, useCallback, useState } from 'react';
import CommandNames from 'Commands/CommandNames';
import { useExecuteCommand } from 'Commands/useCommands';
import FieldSet from 'Components/FieldSet';
import FormInput from 'Components/Form/FormInput';
import FormInputButton from 'Components/Form/FormInputButton';
import FormInputHelpText from 'Components/Form/FormInputHelpText';
import FormLabel from 'Components/Form/FormLabel';
import FormRow from 'Components/Form/FormRow';
import { EnhancedSelectInputValue } from 'Components/Form/Select/EnhancedSelectInput';
import Icon from 'Components/Icon';
import ClipboardButton from 'Components/Link/ClipboardButton';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import { icons, inputTypes, kinds } from 'Helpers/Props';
import { InputChanged } from 'typings/inputs';
import { PendingSection } from 'typings/pending';
import translate from 'Utilities/String/translate';
import { GeneralSettingsModel } from './useGeneralSettings';

export const authenticationMethodOptions: EnhancedSelectInputValue<string>[] = [
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
    isDisabled: true,
    isHidden: true,
  },
  {
    key: 'forms',
    get value() {
      return translate('AuthForm');
    },
  },
];

export const authenticationRequiredOptions: EnhancedSelectInputValue<string>[] =
  [
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
      key: 'disabledForLocalHost',
      get value() {
        return translate('DisabledForLocalhost');
      },
    },
  ];

const certificateValidationOptions: EnhancedSelectInputValue<string>[] = [
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
  authenticationMethod: PendingSection<GeneralSettingsModel>['authenticationMethod'];
  authenticationRequired: PendingSection<GeneralSettingsModel>['authenticationRequired'];
  username: PendingSection<GeneralSettingsModel>['username'];
  password: PendingSection<GeneralSettingsModel>['password'];
  passwordConfirmation: PendingSection<GeneralSettingsModel>['passwordConfirmation'];
  apiKey: PendingSection<GeneralSettingsModel>['apiKey'];
  certificateValidation: PendingSection<GeneralSettingsModel>['certificateValidation'];
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
  const executeCommand = useExecuteCommand();

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

    executeCommand({ name: CommandNames.ResetApiKey });
  }, [executeCommand]);

  const handleCloseResetApiKeyModal = useCallback(() => {
    setIsConfirmApiKeyResetModalOpen(false);
  }, []);

  // createCommandExecutingSelector(CommandNames.RESET_API_KEY),

  const authenticationEnabled =
    authenticationMethod && authenticationMethod.value !== 'none';

  return (
    <FieldSet
      legend={translate('Security')}
      caption={translate('SecurityCaption')}
    >
      <FormRow>
        <FormLabel>{translate('Authentication')}</FormLabel>
        <FormInputHelpText text={translate('AuthenticationMethodHelpText')} />
        <FormInputHelpText
          text={translate('AuthenticationRequiredWarning')}
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
      {authenticationEnabled ? (
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
      ) : null}
      {authenticationEnabled ? (
        <FormRow>
          <FormLabel>{translate('Username')}</FormLabel>

          <FormInput
            type={inputTypes.TEXT}
            name="username"
            onChange={onInputChange}
            {...username}
          />
        </FormRow>
      ) : null}
      {authenticationEnabled ? (
        <FormRow>
          <FormLabel>{translate('Password')}</FormLabel>

          <FormInput
            type={inputTypes.PASSWORD}
            name="password"
            onChange={onInputChange}
            {...password}
          />
        </FormRow>
      ) : null}
      {authenticationEnabled ? (
        <FormRow>
          <FormLabel>{translate('PasswordConfirmation')}</FormLabel>

          <FormInput
            type={inputTypes.PASSWORD}
            name="passwordConfirmation"
            onChange={onInputChange}
            {...passwordConfirmation}
          />
        </FormRow>
      ) : null}
      <FormRow>
        <FormLabel>{translate('ApiKey')}</FormLabel>
        <FormInputHelpText
          text={translate('RestartRequiredHelpTextWarning')}
          isWarning={true}
        />
        <FormInput
          type={inputTypes.TEXT}
          name="apiKey"
          readOnly={true}
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
      </FormRow>
      <FormRow>
        <FormLabel>{translate('CertificateValidation')}</FormLabel>
        <FormInputHelpText text={translate('CertificateValidationHelpText')} />
        <FormInput
          type={inputTypes.SELECT}
          name="certificateValidation"
          values={certificateValidationOptions}
          onChange={onInputChange}
          {...certificateValidation}
        />
      </FormRow>
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
