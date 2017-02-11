import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { icons, inputTypes, kinds, sizes } from 'Helpers/Props';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import FieldSet from 'Components/FieldSet';
import Icon from 'Components/Icon';
import ClipboardButton from 'Components/Link/ClipboardButton';
import PageContent from 'Components/Page/PageContent';
import PageContentBodyConnector from 'Components/Page/PageContentBodyConnector';
import SettingsToolbarConnector from 'Settings/SettingsToolbarConnector';
import Form from 'Components/Form/Form';
import FormGroup from 'Components/Form/FormGroup';
import FormLabel from 'Components/Form/FormLabel';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormInputButton from 'Components/Form/FormInputButton';
import ConfirmModal from 'Components/Modal/ConfirmModal';

class GeneralSettings extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isConfirmApiKeyResetModalOpen: false,
      isRestartRequiredModalOpen: false
    };
  }

  componentDidUpdate(prevProps) {
    const {
      settings,
      isSaving,
      saveError
    } = this.props;

    if (isSaving || saveError || !prevProps.isSaving) {
      return;
    }

    const prevSettings = prevProps.settings;

    const keys = [
      'bindAddress',
      'port',
      'urlBase',
      'enableSsl',
      'sslPort',
      'sslCertHash',
      'authenticationMethod',
      'username',
      'password',
      'apiKey'
    ];

    const pendingRestart = _.some(keys, (key) => {
      const setting = settings[key];
      const prevSetting = prevSettings[key];

      if (!setting || !prevSetting) {
        return false;
      }

      const previousValue = prevSetting.previousValue;
      const value = setting.value;

      return previousValue != null && previousValue !== value;
    });

    this.setState({ isRestartRequiredModalOpen: pendingRestart });
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

  onConfirmRestart = () => {
    this.setState({ isRestartRequiredModalOpen: false });
    this.props.onConfirmRestart();
  }

  onCloseRestartRequiredModalOpen = () => {
    this.setState({ isRestartRequiredModalOpen: false });
  }

  //
  // Render

  render() {
    const {
      advancedSettings,
      isFetching,
      isPopulated,
      error,
      settings,
      hasSettings,
      isResettingApiKey,
      isMono,
      isWindows,
      mode,
      onInputChange,
      ...otherProps
    } = this.props;

    const {
      isConfirmApiKeyResetModalOpen,
      isRestartRequiredModalOpen
    } = this.state;

    const {
      bindAddress,
      port,
      urlBase,
      enableSsl,
      sslPort,
      sslCertHash,
      launchBrowser,
      authenticationMethod,
      username,
      password,
      apiKey,
      proxyEnabled,
      proxyType,
      proxyHostname,
      proxyPort,
      proxyUsername,
      proxyPassword,
      proxyBypassFilter,
      proxyBypassLocalAddresses,
      logLevel,
      analyticsEnabled,
      branch,
      updateAutomatically,
      updateMechanism,
      updateScriptPath
    } = settings;

    const authenticationMethodOptions = [
      { key: 'none', value: 'None' },
      { key: 'basic', value: 'Basic (Browser Popup)' },
      { key: 'forms', value: 'Forms (Login Page)' }
    ];

    const proxyTypeOptions = [
      { key: 'http', value: 'HTTP(S)' },
      { key: 'socks4', value: 'Socks4' },
      { key: 'socks5', value: 'Socks5 (Support TOR)' }
    ];

    const logLevelOptions = [
      { key: 'info', value: 'Info' },
      { key: 'debug', value: 'Debug' },
      { key: 'trace', value: 'Trace' }
    ];

    const updateOptions = [
      { key: 'builtIn', value: 'Built-In' },
      { key: 'script', value: 'Script' }
    ];

    const authenticationEnabled = authenticationMethod && authenticationMethod.value !== 'none';

    return (
      <PageContent title="General Settings">
        <SettingsToolbarConnector
          {...otherProps}
        />

        <PageContentBodyConnector>
          {
            isFetching && !isPopulated &&
              <LoadingIndicator />
          }

          {
            !isFetching && error &&
              <div>Unable to load General settings</div>
          }

          {
            hasSettings && isPopulated && !error &&
              <Form
                id="generalSettings"
                {...otherProps}
              >
                <FieldSet
                  legend="Start-Up"
                >
                  <FormGroup
                    advancedSettings={advancedSettings}
                    isAdvanced={true}
                  >
                    <FormLabel>Bind Address</FormLabel>

                    <FormInputGroup
                      type={inputTypes.TEXT}
                      name="bindAddress"
                      helpText="Valid IP4 address or '*' for all interfaces"
                      helpTextWarning="Requires restart to take effect"
                      onChange={onInputChange}
                      {...bindAddress}
                    />
                  </FormGroup>

                  <FormGroup>
                    <FormLabel>Port Number</FormLabel>

                    <FormInputGroup
                      type={inputTypes.NUMBER}
                      name="port"
                      helpTextWarning="Requires restart to take effect"
                      onChange={onInputChange}
                      {...port}
                    />
                  </FormGroup>

                  <FormGroup>
                    <FormLabel>URL Base</FormLabel>

                    <FormInputGroup
                      type={inputTypes.TEXT}
                      name="urlBase"
                      helpText="For reverse proxy support, default is empty"
                      helpTextWarning="Requires restart to take effect"
                      onChange={onInputChange}
                      {...urlBase}
                    />
                  </FormGroup>

                  <FormGroup
                    advancedSettings={advancedSettings}
                    isAdvanced={true}
                    size={sizes.MEDIUM}
                  >
                    <FormLabel>Enable SSL</FormLabel>

                    <FormInputGroup
                      type={inputTypes.CHECK}
                      name="enableSsl"
                      helpText=" Requires restart running as administrator to take effect"
                      onChange={onInputChange}
                      {...enableSsl}
                    />
                  </FormGroup>

                  {
                    enableSsl.value &&
                      <FormGroup
                        advancedSettings={advancedSettings}
                        isAdvanced={true}
                      >
                        <FormLabel>SSL Port</FormLabel>

                        <FormInputGroup
                          type={inputTypes.NUMBER}
                          name="sslPort"
                          helpTextWarning="Requires restart to take effect"
                          onChange={onInputChange}
                          {...sslPort}
                        />
                      </FormGroup>
                  }

                  {
                    isWindows && enableSsl.value &&
                      <FormGroup
                        advancedSettings={advancedSettings}
                        isAdvanced={true}
                      >
                        <FormLabel>SSL Cert Hash</FormLabel>

                        <FormInputGroup
                          type={inputTypes.TEXT}
                          name="sslCertHash"
                          helpTextWarning="Requires restart to take effect"
                          onChange={onInputChange}
                          {...sslCertHash}
                        />
                      </FormGroup>
                  }

                  {
                    mode !== 'service' &&
                      <FormGroup size={sizes.MEDIUM}>
                        <FormLabel>Open browser on start</FormLabel>

                        <FormInputGroup
                          type={inputTypes.CHECK}
                          name="launchBrowser"
                          helpText=" Open a web browser and navigate to Sonarr homepage on app start."
                          onChange={onInputChange}
                          {...launchBrowser}
                        />
                      </FormGroup>
                  }

                </FieldSet>

                <FieldSet
                  legend="Security"
                >
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
                          <Icon name={isResettingApiKey ? `${icons.REFRESH} fa-spin` : icons.REFRESH} />
                        </FormInputButton>
                      ]}
                      onChange={onInputChange}
                      onFocus={this.onApikeyFocus}
                      {...apiKey}
                    />
                  </FormGroup>
                </FieldSet>

                <FieldSet
                  legend="Proxy Settings"
                >
                  <FormGroup size={sizes.MEDIUM}>
                    <FormLabel>Use Proxy</FormLabel>

                    <FormInputGroup
                      type={inputTypes.CHECK}
                      name="proxyEnabled"
                      onChange={onInputChange}
                      {...proxyEnabled}
                    />
                  </FormGroup>

                  {
                    proxyEnabled.value &&
                      <div>
                        <FormGroup>
                          <FormLabel>Proxy Type</FormLabel>

                          <FormInputGroup
                            type={inputTypes.SELECT}
                            name="proxyType"
                            values={proxyTypeOptions}
                            onChange={onInputChange}
                            {...proxyType}
                          />
                        </FormGroup>

                        <FormGroup>
                          <FormLabel>Hostname</FormLabel>

                          <FormInputGroup
                            type={inputTypes.TEXT}
                            name="proxyHostname"
                            onChange={onInputChange}
                            {...proxyHostname}
                          />
                        </FormGroup>

                        <FormGroup>
                          <FormLabel>Port</FormLabel>

                          <FormInputGroup
                            type={inputTypes.NUMBER}
                            name="proxyPort"
                            onChange={onInputChange}
                            {...proxyPort}
                          />
                        </FormGroup>

                        <FormGroup>
                          <FormLabel>Username</FormLabel>

                          <FormInputGroup
                            type={inputTypes.TEXT}
                            name="proxyUsername"
                            helpText="You only need to enter a username and password if one is required. Leave them blank otherwise."
                            onChange={onInputChange}
                            {...proxyUsername}
                          />
                        </FormGroup>

                        <FormGroup>
                          <FormLabel>Password</FormLabel>

                          <FormInputGroup
                            type={inputTypes.PASSWORD}
                            name="proxyPassword"
                            helpText="You only need to enter a username and password if one is required. Leave them blank otherwise."
                            onChange={onInputChange}
                            {...proxyPassword}
                          />
                        </FormGroup>

                        <FormGroup>
                          <FormLabel>Ignored Addresses</FormLabel>

                          <FormInputGroup
                            type={inputTypes.TEXT}
                            name="proxyBypassFilter"
                            helpText="Use ',' as a separator, and '*.' as a wildcard for subdomains"
                            onChange={onInputChange}
                            {...proxyBypassFilter}
                          />
                        </FormGroup>

                        <FormGroup size={sizes.MEDIUM}>
                          <FormLabel>Bypass Proxy for Local Addresses</FormLabel>

                          <FormInputGroup
                            type={inputTypes.CHECK}
                            name="proxyBypassLocalAddresses"
                            onChange={onInputChange}
                            {...proxyBypassLocalAddresses}
                          />
                        </FormGroup>
                      </div>
                  }
                </FieldSet>

                <FieldSet
                  legend="Logging"
                >
                  <FormGroup>
                    <FormLabel>Log Level</FormLabel>

                    <FormInputGroup
                      type={inputTypes.SELECT}
                      name="logLevel"
                      values={logLevelOptions}
                      helpTextWarning={logLevel.value === 'trace' ? 'Trace logging should only be enabled temporarily' : undefined}
                      onChange={onInputChange}
                      {...logLevel}
                    />
                  </FormGroup>
                </FieldSet>

                <FieldSet
                  legend="Analytics"
                >
                  <FormGroup size={sizes.MEDIUM}>
                    <FormLabel>Send Anonymous Usage Data</FormLabel>

                    <FormInputGroup
                      type={inputTypes.CHECK}
                      name="analyticsEnabled"
                      helpText="Send anonymous usage and error information to Sonarr's servers. This includes information on your browser, which Sonarr WebUI pages you use, error reporting as well as OS and runtime version. We will use this information to prioritize features and bug fixes."
                      helpTextWarning="Requires restart to take effect"
                      onChange={onInputChange}
                      {...analyticsEnabled}
                    />
                  </FormGroup>
                </FieldSet>

                {
                  advancedSettings &&
                    <FieldSet
                      legend="Updates"
                    >
                      <FormGroup
                        advancedSettings={advancedSettings}
                        isAdvanced={true}
                      >
                        <FormLabel>Branch</FormLabel>

                        <FormInputGroup
                          type={inputTypes.TEXT}
                          name="branch"
                          helpText="Branch to use to update Sonarr"
                          helpLink="https://github.com/Sonarr/Sonarr/wiki/Release-Branches"
                          onChange={onInputChange}
                          {...branch}
                        />
                      </FormGroup>

                      {
                        isMono &&
                          <div>
                            <FormGroup
                              advancedSettings={advancedSettings}
                              isAdvanced={true}
                              size={sizes.MEDIUM}
                            >
                              <FormLabel>Automatic</FormLabel>

                              <FormInputGroup
                                type={inputTypes.CHECK}
                                name="updateAutomatically"
                                helpText="Automatically download and install updates. You will still be able to install from System: Updates"
                                onChange={onInputChange}
                                {...updateAutomatically}
                              />
                            </FormGroup>

                            <FormGroup
                              advancedSettings={advancedSettings}
                              isAdvanced={true}
                            >
                              <FormLabel>Mechanism</FormLabel>

                              <FormInputGroup
                                type={inputTypes.SELECT}
                                name="updateMechanism"
                                values={updateOptions}
                                helpText="Use Sonarr's built-in updater or a script"
                                helpLink="https://github.com/Sonarr/Sonarr/wiki/Updating"
                                onChange={onInputChange}
                                {...updateMechanism}
                              />
                            </FormGroup>

                            {
                              updateMechanism.value === 'script' &&
                                <FormGroup
                                  advancedSettings={advancedSettings}
                                  isAdvanced={true}
                                >
                                  <FormLabel>Script Path</FormLabel>

                                  <FormInputGroup
                                    type={inputTypes.TEXT}
                                    name="updateScriptPath"
                                    helpText="Path to a custom script that takes an extracted update package and handle the remainder of the update process"
                                    onChange={onInputChange}
                                    {...updateScriptPath}
                                  />
                                </FormGroup>
                            }
                          </div>
                      }
                    </FieldSet>
                }
              </Form>
          }
        </PageContentBodyConnector>

        <ConfirmModal
          isOpen={isConfirmApiKeyResetModalOpen}
          kind={kinds.DANGER}
          title="Reset API Key"
          message="Are you sure you want to reset your API Key?"
          confirmLabel="Reset"
          onConfirm={this.onConfirmResetApiKey}
          onCancel={this.onCloseResetApiKeyModal}
        />

        <ConfirmModal
          isOpen={isRestartRequiredModalOpen}
          kind={kinds.DANGER}
          title="Restart Sonarr"
          message="Sonarr requires a restart to apply changes, do you want to restart now?"
          cancelLabel="I'll restart later"
          confirmLabel="Restart Now"
          onConfirm={this.onConfirmRestart}
          onCancel={this.onCloseRestartRequiredModalOpen}
        />
      </PageContent>
    );
  }

}

GeneralSettings.propTypes = {
  advancedSettings: PropTypes.bool.isRequired,
  isFetching: PropTypes.bool.isRequired,
  isPopulated: PropTypes.bool.isRequired,
  error: PropTypes.object,
  isSaving: PropTypes.bool.isRequired,
  saveError: PropTypes.object,
  settings: PropTypes.object.isRequired,
  isResettingApiKey: PropTypes.bool.isRequired,
  hasSettings: PropTypes.bool.isRequired,
  isMono: PropTypes.bool.isRequired,
  isWindows: PropTypes.bool.isRequired,
  mode: PropTypes.string.isRequired,
  onInputChange: PropTypes.func.isRequired,
  onConfirmResetApiKey: PropTypes.func.isRequired,
  onConfirmRestart: PropTypes.func.isRequired
};

export default GeneralSettings;
