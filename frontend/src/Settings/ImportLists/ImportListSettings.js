import PropTypes from 'prop-types';
import React, { Component, Fragment } from 'react';
import { icons } from 'Helpers/Props';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import PageToolbarButton from 'Components/Page/Toolbar/PageToolbarButton';
import PageToolbarSeparator from 'Components/Page/Toolbar/PageToolbarSeparator';
import SettingsToolbarConnector from 'Settings/SettingsToolbarConnector';
import ImportListsConnector from './ImportLists/ImportListsConnector';
import ImportListsExclusionsConnector from './ImportListExclusions/ImportListExclusionsConnector';

class ImportListSettings extends Component {

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

  setListOptionsRef = (ref) => {
    this._listOptions = ref;
  }

  onHasPendingChange = (hasPendingChanges) => {
    this.setState({
      hasPendingChanges
    });
  }

  onSavePress = () => {
    this._listOptions.getWrappedInstance().save();
  }

  //
  // Render

  render() {
    const {
      isTestingAll,
      dispatchTestAllImportLists
    } = this.props;

    const {
      isSaving,
      hasPendingChanges
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
            </Fragment>
          }
          onSavePress={this.onSavePress}
        />

        <PageContentBody>
          <ImportListsConnector />
          <ImportListsExclusionsConnector />
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
