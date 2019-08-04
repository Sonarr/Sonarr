import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { icons, kinds, sizes } from 'Helpers/Props';
import Button from 'Components/Link/Button';
import Icon from 'Components/Icon';
import FileBrowserModal from 'Components/FileBrowser/FileBrowserModal';
import styles from './AddRootFolder.css';

class AddRootFolder extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isAddNewRootFolderModalOpen: false
    };
  }

  //
  // Lifecycle

  onAddNewRootFolderPress = () => {
    this.setState({ isAddNewRootFolderModalOpen: true });
  }

  onNewRootFolderSelect = ({ value }) => {
    this.props.onNewRootFolderSelect(value);
  }

  onAddRootFolderModalClose = () => {
    this.setState({ isAddNewRootFolderModalOpen: false });
  }

  //
  // Render

  render() {
    return (
      <div className={styles.addRootFolderButtonContainer}>
        <Button
          kind={kinds.PRIMARY}
          size={sizes.LARGE}
          onPress={this.onAddNewRootFolderPress}
        >
          <Icon
            className={styles.importButtonIcon}
            name={icons.DRIVE}
          />
          Add Root Folder
        </Button>

        <FileBrowserModal
          isOpen={this.state.isAddNewRootFolderModalOpen}
          name="rootFolderPath"
          value=""
          onChange={this.onNewRootFolderSelect}
          onModalClose={this.onAddRootFolderModalClose}
        />
      </div>
    );
  }
}

AddRootFolder.propTypes = {
  onNewRootFolderSelect: PropTypes.func.isRequired
};

export default AddRootFolder;
