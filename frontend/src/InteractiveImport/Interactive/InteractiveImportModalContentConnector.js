import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { fetchInteractiveImportItems, setInteractiveImportSort, clearInteractiveImport, setInteractiveImportMode } from 'Store/Actions/interactiveImportActions';
import createClientSideCollectionSelector from 'Store/Selectors/createClientSideCollectionSelector';
import { executeCommand } from 'Store/Actions/commandActions';
import * as commandNames from 'Commands/commandNames';
import InteractiveImportModalContent from './InteractiveImportModalContent';

function createMapStateToProps() {
  return createSelector(
    createClientSideCollectionSelector('interactiveImport'),
    (interactiveImport) => {
      return interactiveImport;
    }
  );
}

const mapDispatchToProps = {
  fetchInteractiveImportItems,
  setInteractiveImportSort,
  setInteractiveImportMode,
  clearInteractiveImport,
  executeCommand
};

class InteractiveImportModalContentConnector extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      interactiveImportErrorMessage: null,
      filterExistingFiles: true
    };
  }

  componentDidMount() {
    const {
      downloadId,
      folder
    } = this.props;

    const {
      filterExistingFiles
    } = this.state;

    this.props.fetchInteractiveImportItems({
      downloadId,
      folder,
      filterExistingFiles
    });
  }

  componentDidUpdate(prevProps, prevState) {
    const {
      filterExistingFiles
    } = this.state;

    if (prevState.filterExistingFiles !== filterExistingFiles) {
      const {
        downloadId,
        folder
      } = this.props;

      this.props.fetchInteractiveImportItems({
        downloadId,
        folder,
        filterExistingFiles
      });
    }
  }

  componentWillUnmount() {
    this.props.clearInteractiveImport();
  }

  //
  // Listeners

  onSortPress = (sortKey, sortDirection) => {
    this.props.setInteractiveImportSort({ sortKey, sortDirection });
  }

  onFilterExistingFilesChange = (filterExistingFiles) => {
    this.setState({ filterExistingFiles });
  }

  onImportModeChange = (importMode) => {
    this.props.setInteractiveImportMode({ importMode });
  }

  onImportSelectedPress = (selected, importMode) => {
    const files = [];

    _.forEach(this.props.items, (item) => {
      const isSelected = selected.indexOf(item.id) > -1;

      if (isSelected) {
        const {
          series,
          seasonNumber,
          episodes,
          quality,
          language
        } = item;

        if (!series) {
          this.setState({ interactiveImportErrorMessage: 'Series must be chosen for each selected file' });
          return false;
        }

        if (isNaN(seasonNumber)) {
          this.setState({ interactiveImportErrorMessage: 'Season must be chosen for each selected file' });
          return false;
        }

        if (!episodes || !episodes.length) {
          this.setState({ interactiveImportErrorMessage: 'One or more episodes must be chosen for each selected file' });
          return false;
        }

        if (!quality) {
          this.setState({ interactiveImportErrorMessage: 'Quality must be chosen for each selected file' });
          return false;
        }

        if (!language) {
          this.setState({ interactiveImportErrorMessage: 'Language must be chosen for each selected file' });
          return false;
        }

        files.push({
          path: item.path,
          folderName: item.folderName,
          seriesId: series.id,
          episodeIds: _.map(episodes, 'id'),
          quality,
          language,
          downloadId: this.props.downloadId
        });
      }
    });

    if (!files.length) {
      return;
    }

    this.props.executeCommand({
      name: commandNames.INTERACTIVE_IMPORT,
      files,
      importMode
    });

    this.props.onModalClose();
  }

  //
  // Render

  render() {
    const {
      interactiveImportErrorMessage,
      filterExistingFiles
    } = this.state;

    return (
      <InteractiveImportModalContent
        {...this.props}
        interactiveImportErrorMessage={interactiveImportErrorMessage}
        filterExistingFiles={filterExistingFiles}
        onSortPress={this.onSortPress}
        onFilterExistingFilesChange={this.onFilterExistingFilesChange}
        onImportModeChange={this.onImportModeChange}
        onImportSelectedPress={this.onImportSelectedPress}
      />
    );
  }
}

InteractiveImportModalContentConnector.propTypes = {
  downloadId: PropTypes.string,
  folder: PropTypes.string,
  filterExistingFiles: PropTypes.bool.isRequired,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  fetchInteractiveImportItems: PropTypes.func.isRequired,
  setInteractiveImportSort: PropTypes.func.isRequired,
  clearInteractiveImport: PropTypes.func.isRequired,
  setInteractiveImportMode: PropTypes.func.isRequired,
  executeCommand: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

InteractiveImportModalContentConnector.defaultProps = {
  filterExistingFiles: true
};

export default connect(createMapStateToProps, mapDispatchToProps)(InteractiveImportModalContentConnector);
