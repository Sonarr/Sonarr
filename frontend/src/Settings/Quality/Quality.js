import React, { Component } from 'react';
import PageContent from 'Components/Page/PageContent';
import PageContentBodyConnector from 'Components/Page/PageContentBodyConnector';
import SettingsToolbarConnector from 'Settings/SettingsToolbarConnector';
import QualityDefinitionsConnector from './Definition/QualityDefinitionsConnector';

class Quality extends Component {

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

  setQualityDefinitionsRef = (ref) => {
    this._qualityDefinitions = ref;
  }

  onHasPendingChange = (hasPendingChanges) => {
    this.setState({
      hasPendingChanges
    });
  }

  onSavePress = () => {
    this._qualityDefinitions.getWrappedInstance().save();
  }

  //
  // Render

  render() {
    return (
      <PageContent title="Quality Settings">
        <SettingsToolbarConnector
          hasPendingChanges={this.state.hasPendingChanges}
          onSavePress={this.onSavePress}
        />

        <PageContentBodyConnector>
          <QualityDefinitionsConnector
            ref={this.setQualityDefinitionsRef}
            onHasPendingChange={this.onHasPendingChange}
          />
        </PageContentBodyConnector>
      </PageContent>
    );
  }
}

export default Quality;
