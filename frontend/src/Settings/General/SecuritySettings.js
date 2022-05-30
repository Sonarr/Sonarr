import PropTypes from 'prop-types';
import React, { Component } from 'react';
import FieldSet from 'Components/FieldSet';
import FormGroup from 'Components/Form/FormGroup';
import FormInputButton from 'Components/Form/FormInputButton';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import Icon from 'Components/Icon';
import ClipboardButton from 'Components/Link/ClipboardButton';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import { icons, inputTypes, kinds } from 'Helpers/Props';

export const authenticationRequiredWarning = 'To prevent remote access without authentication, Sonarr now requires authentication to be enabled. You can optionally disable authentication from local addresses.';

export const authenticationMethodOptions = [
  { key: 'none', value: 'None', isDisabled: true },
  { key: 'basic', value: 'Basic (Browser Popup)' },
  { key: 'forms', value: 'Forms (Login Page)' }
];

export const authenticationRequiredOptions = [
  { key: 'enabled', value: 'Enabled' },
  { key: 'disabledForLocalAddresses', value: 'Disabled for Local Addresses' }
];

const certificateValidationOptions = [
  { key: 'enabled', value: 'Enabled' },
  { key: 'disabledForLocalAddresses', value: 'Disabled for Local Addresses' },
  { key: 'disabled', value: 'Disabled' }
];

class SecuritySettings extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isConfirmApiKeyResetModalOpen: false
    };
  }

  //
  // Listeners

  onApikeyFocus = (event) => {
    event.target.select();
  };

  onResetApiKeyPress = () => {
    this.setState({ isConfirmApiKeyResetModalOpen: true });
  };

  onConfirmResetApiKey = () => {
    this.setState({ isConfirmApiKeyResetModalOpen: false });
    this.props.onConfirmResetApiKey();
  };

  onCloseResetApiKeyModal = () => {
    this.setState({ isConfirmApiKeyResetModalOpen: false });
  };

  //
  // Render

  render() {
    const {
      settings,
      isResettingApiKey,
      onInputChange
    } = this.props;

    const {
      authenticationMethod,
      authenticationRequired,
      username,
      password,
      apiKey,
      certificateValidation
    } = settings;

    const authenticationEnabled = authenticationMethod && authenticationMethod.value !== 'none';

    return (
      <FieldSet legend="Security">
        <FormGroup>
          <FormLabel>Authentication</FormLabel>

          <FormInputGroup
            type={inputTypes.SELECT}
            name="authenticationMethod"
            values={authenticationMethodOptions}
            helpText="Require Username and Password to access Sonarr"
            helpTextWarning={authenticationRequiredWarning}
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

        <FormGroup>
          <FormLabel>API Key</FormLabel>

          <FormInputGroup
            type={inputTypes.TEXT}
            name="apiKey"
            readOnly={true}
            helpTextWarning="Requires restart to take effect"
            buttons={[
              <ClipboardButton
                key="copy"
                value={apiKey.value}
                kind={kinds.DEFAULT}
              />,

              <FormInputButton
                key="reset"
                kind={kinds.DANGER}
                onPress={this.onResetApiKeyPress}
              >
                <Icon
                  name={icons.REFRESH}
                  isSpinning={isResettingApiKey}
                />
              </FormInputButton>
            ]}
            onChange={onInputChange}
            onFocus={this.onApikeyFocus}
            {...apiKey}
          />
        </FormGroup>

        <FormGroup>
          <FormLabel>Certificate Validation</FormLabel>

          <FormInputGroup
            type={inputTypes.SELECT}
            name="certificateValidation"
            values={certificateValidationOptions}
            helpText="Change how strict HTTPS certification validation is. Do not change unless you understand the risks."
            onChange={onInputChange}
            {...certificateValidation}
          />
        </FormGroup>

        <ConfirmModal
          isOpen={this.state.isConfirmApiKeyResetModalOpen}
          kind={kinds.DANGER}
          title="Reset API Key"
          message="Are you sure you want to reset your API Key?"
          confirmLabel="Reset"
          onConfirm={this.onConfirmResetApiKey}
          onCancel={this.onCloseResetApiKeyModal}
        />
      </FieldSet>
    );
  }
}

SecuritySettings.propTypes = {
  settings: PropTypes.object.isRequired,
  isResettingApiKey: PropTypes.bool.isRequired,
  onInputChange: PropTypes.func.isRequired,
  onConfirmResetApiKey: PropTypes.func.isRequired
};

export default SecuritySettings;
