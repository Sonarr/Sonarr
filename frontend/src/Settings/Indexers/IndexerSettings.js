import React, { Component } from 'react';
import PageContent from 'Components/Page/PageContent';
import PageContentBodyConnector from 'Components/Page/PageContentBodyConnector';
import SettingsToolbarConnector from 'Settings/SettingsToolbarConnector';
import IndexersConnector from './Indexers/IndexersConnector';
import IndexerOptionsConnector from './Options/IndexerOptionsConnector';
import RestrictionsConnector from './Restrictions/RestrictionsConnector';

class IndexerSettings extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      hasPendingChanges: false
    };
  }

  //
  // Listeners

  setIndexerOptionsRef = (ref) => {
    this._indexerOptions = ref;
  }

  onHasPendingChange = (hasPendingChanges) => {
    this.setState({
      hasPendingChanges
    });
  }

  onSavePress = () => {
    this._indexerOptions.getWrappedInstance().save();
  }

  //
  // Render

  render() {
    return (
      <PageContent title="Indexer Settings">
        <SettingsToolbarConnector
          hasPendingChanges={this.state.hasPendingChanges}
          onSavePress={this.onSavePress}
        />

        <PageContentBodyConnector>
          <IndexersConnector />

          <IndexerOptionsConnector
            ref={this.setIndexerOptionsRef}
            onHasPendingChange={this.onHasPendingChange}
          />

          <RestrictionsConnector />
        </PageContentBodyConnector>
      </PageContent>
    );
  }
}

export default IndexerSettings;
