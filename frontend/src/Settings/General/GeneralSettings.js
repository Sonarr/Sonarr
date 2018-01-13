import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { kinds } from 'Helpers/Props';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import PageContent from 'Components/Page/PageContent';
import PageContentBodyConnector from 'Components/Page/PageContentBodyConnector';
import SettingsToolbarConnector from 'Settings/SettingsToolbarConnector';
import Form from 'Components/Form/Form';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import AnalyticSettings from './AnalyticSettings';
import BackupSettings from './BackupSettings';
import HostSettings from './HostSettings';
import LoggingSettings from './LoggingSettings';
import ProxySettings from './ProxySettings';
import SecuritySettings from './SecuritySettings';
import UpdateSettings from './UpdateSettings';

class GeneralSettings extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
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
      onConfirmResetApiKey,
      ...otherProps
    } = this.props;

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
                <HostSettings
                  advancedSettings={advancedSettings}
                  settings={settings}
                  isWindows={isWindows}
                  mode={mode}
                  onInputChange={onInputChange}
                />

                <SecuritySettings
                  settings={settings}
                  isResettingApiKey={isResettingApiKey}
                  onInputChange={onInputChange}
                  onConfirmResetApiKey={onConfirmResetApiKey}
                />

                <ProxySettings
                  settings={settings}
                  onInputChange={onInputChange}
                />

                <LoggingSettings
                  settings={settings}
                  onInputChange={onInputChange}
                />

                <AnalyticSettings
                  settings={settings}
                  onInputChange={onInputChange}
                />

                <UpdateSettings
                  advancedSettings={advancedSettings}
                  settings={settings}
                  isMono={isMono}
                  onInputChange={onInputChange}
                />

                <BackupSettings
                  advancedSettings={advancedSettings}
                  settings={settings}
                  onInputChange={onInputChange}
                />
              </Form>
          }
        </PageContentBodyConnector>

        <ConfirmModal
          isOpen={this.state.isRestartRequiredModalOpen}
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
