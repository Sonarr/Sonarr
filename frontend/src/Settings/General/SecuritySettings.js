import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { icons, kinds, inputTypes } from 'Helpers/Props';
import FieldSet from 'Components/FieldSet';
import Icon from 'Components/Icon';
import ClipboardButton from 'Components/Link/ClipboardButton';
import FormGroup from 'Components/Form/FormGroup';
import FormLabel from 'Components/Form/FormLabel';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormInputButton from 'Components/Form/FormInputButton';
import ConfirmModal from 'Components/Modal/ConfirmModal';

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
  }

  onResetApiKeyPress = () => {
    this.setState({ isConfirmApiKeyResetModalOpen: true });
  }

  onConfirmResetApiKey = () => {
    this.setState({ isConfirmApiKeyResetModalOpen: false });
    this.props.onConfirmResetApiKey();
  }

  onCloseResetApiKeyModal = () => {
    this.setState({ isConfirmApiKeyResetModalOpen: false });
  }

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
      username,
      password,
      apiKey
    } = settings;

    const authenticationMethodOptions = [
      { key: 'none', value: 'None' },
      { key: 'basic', value: 'Basic (Browser Popup)' },
      { key: 'forms', value: 'Forms (Login Page)' }
    ];

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
            helpTextWarning="Requires restart to take effect"
            onChange={onInputChange}
            {...authenticationMethod}
          />
        </FormGroup>

        {
          authenticationEnabled &&
          <FormGroup>
            <FormLabel>Username</FormLabel>

            <FormInputGroup
              type={inputTypes.TEXT}
              name="username"
              helpTextWarning="Requires restart to take effect"
              onChange={onInputChange}
              {...username}
            />
          </FormGroup>
        }

        {
          authenticationEnabled &&
          <FormGroup>
            <FormLabel>Password</FormLabel>

            <FormInputGroup
              type={inputTypes.PASSWORD}
              name="password"
              helpTextWarning="Requires restart to take effect"
              onChange={onInputChange}
              {...password}
            />
          </FormGroup>
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
