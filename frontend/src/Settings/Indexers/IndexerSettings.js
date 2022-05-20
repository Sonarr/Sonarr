import PropTypes from 'prop-types';
import React, { Component, Fragment } from 'react';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import PageToolbarButton from 'Components/Page/Toolbar/PageToolbarButton';
import PageToolbarSeparator from 'Components/Page/Toolbar/PageToolbarSeparator';
import { icons } from 'Helpers/Props';
import SettingsToolbarConnector from 'Settings/SettingsToolbarConnector';
import IndexersConnector from './Indexers/IndexersConnector';
import IndexerOptionsConnector from './Options/IndexerOptionsConnector';

class IndexerSettings extends Component {

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
  };

  onChildStateChange = (payload) => {
    this.setState(payload);
  };

  onSavePress = () => {
    if (this._saveCallback) {
      this._saveCallback();
    }
  };

  // Render
  //

  render() {
    const {
      isTestingAll,
      dispatchTestAllIndexers
    } = this.props;

    const {
      isSaving,
      hasPendingChanges
    } = this.state;

    return (
      <PageContent title="Indexer Settings">
        <SettingsToolbarConnector
          isSaving={isSaving}
          hasPendingChanges={hasPendingChanges}
          additionalButtons={
            <Fragment>
              <PageToolbarSeparator />

              <PageToolbarButton
                label="Test All Indexers"
                iconName={icons.TEST}
                isSpinning={isTestingAll}
                onPress={dispatchTestAllIndexers}
              />
            </Fragment>
          }
          onSavePress={this.onSavePress}
        />

        <PageContentBody>
          <IndexersConnector />

          <IndexerOptionsConnector
            onChildMounted={this.onChildMounted}
            onChildStateChange={this.onChildStateChange}
          />
        </PageContentBody>
      </PageContent>
    );
  }
}

IndexerSettings.propTypes = {
  isTestingAll: PropTypes.bool.isRequired,
  dispatchTestAllIndexers: PropTypes.func.isRequired
};

export default IndexerSettings;
