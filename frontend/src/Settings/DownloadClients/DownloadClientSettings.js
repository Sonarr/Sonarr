import PropTypes from 'prop-types';
import React, { Component, Fragment } from 'react';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import PageToolbarButton from 'Components/Page/Toolbar/PageToolbarButton';
import PageToolbarSeparator from 'Components/Page/Toolbar/PageToolbarSeparator';
import { icons } from 'Helpers/Props';
import SettingsToolbarConnector from 'Settings/SettingsToolbarConnector';
import translate from 'Utilities/String/translate';
import DownloadClientsConnector from './DownloadClients/DownloadClientsConnector';
import ManageDownloadClientsModal from './DownloadClients/Manage/ManageDownloadClientsModal';
import DownloadClientOptionsConnector from './Options/DownloadClientOptionsConnector';
import RemotePathMappingsConnector from './RemotePathMappings/RemotePathMappingsConnector';

class DownloadClientSettings extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this._saveCallback = null;

    this.state = {
      isSaving: false,
      hasPendingChanges: false,
      isManageDownloadClientsOpen: false
    };
  }

  //
  // Listeners

  onChildMounted = (saveCallback) => {
    this._saveCallback = saveCallback;
  };

  onChildStateChange = (payload) => {
    this.setState(payload);
  };

  onManageDownloadClientsPress = () => {
    this.setState({ isManageDownloadClientsOpen: true });
  };

  onManageDownloadClientsModalClose = () => {
    this.setState({ isManageDownloadClientsOpen: false });
  };

  onSavePress = () => {
    if (this._saveCallback) {
      this._saveCallback();
    }
  };

  //
  // Render

  render() {
    const {
      isTestingAll,
      dispatchTestAllDownloadClients
    } = this.props;

    const {
      isSaving,
      hasPendingChanges,
      isManageDownloadClientsOpen
    } = this.state;

    return (
      <PageContent title={translate('DownloadClientSettings')}>
        <SettingsToolbarConnector
          isSaving={isSaving}
          hasPendingChanges={hasPendingChanges}
          additionalButtons={
            <Fragment>
              <PageToolbarSeparator />

              <PageToolbarButton
                label={translate('TestAllClients')}
                iconName={icons.TEST}
                isSpinning={isTestingAll}
                onPress={dispatchTestAllDownloadClients}
              />

              <PageToolbarButton
                label={translate('ManageClients')}
                iconName={icons.MANAGE}
                onPress={this.onManageDownloadClientsPress}
              />
            </Fragment>
          }
          onSavePress={this.onSavePress}
        />

        <PageContentBody>
          <DownloadClientsConnector />

          <DownloadClientOptionsConnector
            onChildMounted={this.onChildMounted}
            onChildStateChange={this.onChildStateChange}
          />

          <RemotePathMappingsConnector />

          <ManageDownloadClientsModal
            isOpen={isManageDownloadClientsOpen}
            onModalClose={this.onManageDownloadClientsModalClose}
          />
        </PageContentBody>
      </PageContent>
    );
  }
}

DownloadClientSettings.propTypes = {
  isTestingAll: PropTypes.bool.isRequired,
  dispatchTestAllDownloadClients: PropTypes.func.isRequired
};

export default DownloadClientSettings;
