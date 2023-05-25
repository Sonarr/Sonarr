import PropTypes from 'prop-types';
import React, { Component, Fragment } from 'react';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import PageToolbarButton from 'Components/Page/Toolbar/PageToolbarButton';
import PageToolbarSeparator from 'Components/Page/Toolbar/PageToolbarSeparator';
import { icons } from 'Helpers/Props';
import SettingsToolbarConnector from 'Settings/SettingsToolbarConnector';
import ImportListsExclusionsConnector from './ImportListExclusions/ImportListExclusionsConnector';
import ImportListsConnector from './ImportLists/ImportListsConnector';
import ManageImportListsModal from './ImportLists/Manage/ManageImportListsModal';

class ImportListSettings extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      hasPendingChanges: false,
      isManageImportListsOpen: false
    };
  }

  //
  // Listeners

  setListOptionsRef = (ref) => {
    this._listOptions = ref;
  };

  onManageImportListsPress = () => {
    this.setState({ isManageImportListsOpen: true });
  };

  onManageImportListsModalClose = () => {
    this.setState({ isManageImportListsOpen: false });
  };

  onHasPendingChange = (hasPendingChanges) => {
    this.setState({
      hasPendingChanges
    });
  };

  onSavePress = () => {
    this._listOptions.getWrappedInstance().save();
  };

  //
  // Render

  render() {
    const {
      isTestingAll,
      dispatchTestAllImportLists
    } = this.props;

    const {
      isSaving,
      hasPendingChanges,
      isManageImportListsOpen
    } = this.state;

    return (
      <PageContent title="Import List Settings">
        <SettingsToolbarConnector
          isSaving={isSaving}
          hasPendingChanges={hasPendingChanges}
          additionalButtons={
            <Fragment>
              <PageToolbarSeparator />

              <PageToolbarButton
                label="Test All Lists"
                iconName={icons.TEST}
                isSpinning={isTestingAll}
                onPress={dispatchTestAllImportLists}
              />

              <PageToolbarButton
                label="Manage Lists"
                iconName={icons.MANAGE}
                onPress={this.onManageImportListsPress}
              />
            </Fragment>
          }
          onSavePress={this.onSavePress}
        />

        <PageContentBody>
          <ImportListsConnector />
          <ImportListsExclusionsConnector />
          <ManageImportListsModal
            isOpen={isManageImportListsOpen}
            onModalClose={this.onManageImportListsModalClose}
          />
        </PageContentBody>
      </PageContent>
    );
  }
}

ImportListSettings.propTypes = {
  isTestingAll: PropTypes.bool.isRequired,
  dispatchTestAllImportLists: PropTypes.func.isRequired
};

export default ImportListSettings;
