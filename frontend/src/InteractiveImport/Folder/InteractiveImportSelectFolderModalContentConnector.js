import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { addRecentFolder } from 'Store/Actions/interactiveImportActions';
import { executeCommand } from 'Store/Actions/commandActions';
import * as commandNames from 'Commands/commandNames';
import InteractiveImportSelectFolderModalContent from './InteractiveImportSelectFolderModalContent';

function createMapStateToProps() {
  return createSelector(
    (state) => state.interactiveImport.recentFolders,
    (recentFolders) => {
      return {
        recentFolders
      };
    }
  );
}

const mapDispatchToProps = {
  addRecentFolder,
  executeCommand
};

class InteractiveImportSelectFolderModalContentConnector extends Component {

  //
  // Listeners

  onQuickImportPress = (folder) => {
    this.props.addRecentFolder({ folder });

    this.props.executeCommand({
      name: commandNames.DOWNLOADED_EPSIODES_SCAN,
      path: folder
    });

    this.props.onModalClose();
  }

  onInteractiveImportPress = (folder) => {
    this.props.addRecentFolder({ folder });
    this.props.onFolderSelect(folder);
  }

  //
  // Render

  render() {
    if (this.path) {
      return null;
    }

    return (
      <InteractiveImportSelectFolderModalContent
        {...this.props}
        onQuickImportPress={this.onQuickImportPress}
        onInteractiveImportPress={this.onInteractiveImportPress}
      />
    );
  }
}

InteractiveImportSelectFolderModalContentConnector.propTypes = {
  path: PropTypes.string,
  onFolderSelect: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired,
  addRecentFolder: PropTypes.func.isRequired,
  executeCommand: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(InteractiveImportSelectFolderModalContentConnector);
