import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { createSelector } from 'reselect';
import connectSection from 'Store/connectSection';
import { fetchInteractiveImportItems, setInteractiveImportSort, clearInteractiveImport, setInteractiveImportMode } from 'Store/Actions/interactiveImportActions';
import createClientSideCollectionSelector from 'Store/Selectors/createClientSideCollectionSelector';
import { executeCommand } from 'Store/Actions/commandActions';
import * as commandNames from 'Commands/commandNames';
import InteractiveImportModalContent from './InteractiveImportModalContent';

function createMapStateToProps() {
  return createSelector(
    createClientSideCollectionSelector(),
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
      interactiveImportErrorMessage: null
    };
  }

  componentDidMount() {
    const {
      downloadId,
      folder
    } = this.props;

    this.props.fetchInteractiveImportItems({ downloadId, folder });
  }

  componentWillUnmount() {
    this.props.clearInteractiveImport();
  }

  //
  // Listeners

  onSortPress = (sortKey, sortDirection) => {
    this.props.setInteractiveImportSort({ sortKey, sortDirection });
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

        files.push({
          path: item.path,
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
    return (
      <InteractiveImportModalContent
        {...this.props}
        interactiveImportErrorMessage={this.state.interactiveImportErrorMessage}
        onSortPress={this.onSortPress}
        onImportModeChange={this.onImportModeChange}
        onImportSelectedPress={this.onImportSelectedPress}
      />
    );
  }
}

InteractiveImportModalContentConnector.propTypes = {
  downloadId: PropTypes.string,
  folder: PropTypes.string,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  fetchInteractiveImportItems: PropTypes.func.isRequired,
  setInteractiveImportSort: PropTypes.func.isRequired,
  clearInteractiveImport: PropTypes.func.isRequired,
  setInteractiveImportMode: PropTypes.func.isRequired,
  executeCommand: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default connectSection(
  createMapStateToProps,
  mapDispatchToProps,
  undefined,
  undefined,
  { section: 'interactiveImport' }
)(InteractiveImportModalContentConnector);
