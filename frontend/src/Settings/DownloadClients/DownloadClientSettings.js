import React, { Component } from 'react';
import PageContent from 'Components/Page/PageContent';
import PageContentBodyConnector from 'Components/Page/PageContentBodyConnector';
import SettingsToolbarConnector from 'Settings/SettingsToolbarConnector';
import DownloadClientsConnector from './DownloadClients/DownloadClientsConnector';
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
      hasPendingChanges: false
    };
  }

  //
  // Listeners

  onChildMounted = (saveCallback) => {
    this._saveCallback = saveCallback;
  }

  onChildStateChange = (payload) => {
    this.setState(payload);
  }

  onSavePress = () => {
    if (this._saveCallback) {
      this._saveCallback();
    }
  }

  //
  // Render

  render() {
    const {
      isSaving,
      hasPendingChanges
    } = this.state;

    return (
      <PageContent title="Download Client Settings">
        <SettingsToolbarConnector
          isSaving={isSaving}
          hasPendingChanges={hasPendingChanges}
          onSavePress={this.onSavePress}
        />

        <PageContentBodyConnector>
          <DownloadClientsConnector />

          <DownloadClientOptionsConnector
            onChildMounted={this.onChildMounted}
            onChildStateChange={this.onChildStateChange}
          />

          <RemotePathMappingsConnector />
        </PageContentBodyConnector>
      </PageContent>
    );
  }
}

export default DownloadClientSettings;
