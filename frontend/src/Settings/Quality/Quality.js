import PropTypes from 'prop-types';
import React, { Component, Fragment } from 'react';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import PageToolbarButton from 'Components/Page/Toolbar/PageToolbarButton';
import PageToolbarSeparator from 'Components/Page/Toolbar/PageToolbarSeparator';
import { icons } from 'Helpers/Props';
import SettingsToolbarConnector from 'Settings/SettingsToolbarConnector';
import translate from 'Utilities/String/translate';
import QualityDefinitionsConnector from './Definition/QualityDefinitionsConnector';
import ResetQualityDefinitionsModal from './Reset/ResetQualityDefinitionsModal';

class Quality extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this._saveCallback = null;

    this.state = {
      isSaving: false,
      hasPendingChanges: false,
      isConfirmQualityDefinitionResetModalOpen: false
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

  onResetQualityDefinitionsPress = () => {
    this.setState({ isConfirmQualityDefinitionResetModalOpen: true });
  };

  onCloseResetQualityDefinitionsModal = () => {
    this.setState({ isConfirmQualityDefinitionResetModalOpen: false });
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
      isSaving,
      isResettingQualityDefinitions,
      hasPendingChanges
    } = this.state;

    return (
      <PageContent title={translate('QualitySettings')}>
        <SettingsToolbarConnector
          isSaving={isSaving}
          hasPendingChanges={hasPendingChanges}
          additionalButtons={
            <Fragment>
              <PageToolbarSeparator />

              <PageToolbarButton
                label={translate('ResetDefinitions')}
                iconName={icons.REFRESH}
                isSpinning={isResettingQualityDefinitions}
                onPress={this.onResetQualityDefinitionsPress}
              />
            </Fragment>
          }
          onSavePress={this.onSavePress}
        />

        <PageContentBody>
          <QualityDefinitionsConnector
            onChildMounted={this.onChildMounted}
            onChildStateChange={this.onChildStateChange}
          />
        </PageContentBody>

        <ResetQualityDefinitionsModal
          isOpen={this.state.isConfirmQualityDefinitionResetModalOpen}
          onModalClose={this.onCloseResetQualityDefinitionsModal}
        />
      </PageContent>
    );
  }
}

Quality.propTypes = {
  isResettingQualityDefinitions: PropTypes.bool.isRequired
};

export default Quality;
