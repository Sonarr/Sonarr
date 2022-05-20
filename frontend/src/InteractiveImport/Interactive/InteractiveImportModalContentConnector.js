import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import * as commandNames from 'Commands/commandNames';
import { sortDirections } from 'Helpers/Props';
import { executeCommand } from 'Store/Actions/commandActions';
import { deleteEpisodeFiles, updateEpisodeFiles } from 'Store/Actions/episodeFileActions';
import { clearInteractiveImport, fetchInteractiveImportItems, setInteractiveImportMode, setInteractiveImportSort } from 'Store/Actions/interactiveImportActions';
import createClientSideCollectionSelector from 'Store/Selectors/createClientSideCollectionSelector';
import hasDifferentItems from 'Utilities/Object/hasDifferentItems';
import InteractiveImportModalContent from './InteractiveImportModalContent';

function isSameEpisodeFile(file, originalFile) {
  const {
    series,
    seasonNumber,
    episodes
  } = file;

  if (!originalFile) {
    return false;
  }

  if (!originalFile.series || series.id !== originalFile.series.id) {
    return false;
  }

  if (seasonNumber !== originalFile.seasonNumber) {
    return false;
  }

  return !hasDifferentItems(originalFile.episodes, episodes);
}

function createMapStateToProps() {
  return createSelector(
    createClientSideCollectionSelector('interactiveImport'),
    (state) => state.episodeFiles.isDeleting,
    (state) => state.episodeFiles.deleteError,
    (interactiveImport, isDeleting, deleteError) => {
      return {
        ...interactiveImport,
        isDeleting,
        deleteError
      };
    }
  );
}

const mapDispatchToProps = {
  dispatchFetchInteractiveImportItems: fetchInteractiveImportItems,
  dispatchSetInteractiveImportSort: setInteractiveImportSort,
  dispatchSetInteractiveImportMode: setInteractiveImportMode,
  dispatchClearInteractiveImport: clearInteractiveImport,
  dispatchUpdateEpisodeFiles: updateEpisodeFiles,
  dispatchDeleteEpisodeFiles: deleteEpisodeFiles,
  dispatchExecuteCommand: executeCommand
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
      seriesId,
      seasonNumber,
      folder,
      initialSortKey,
      initialSortDirection,
      dispatchSetInteractiveImportSort,
      dispatchFetchInteractiveImportItems
    } = this.props;

    const {
      filterExistingFiles
    } = this.state;

    if (initialSortKey) {
      const sortProps = {
        sortKey: initialSortKey
      };

      if (initialSortDirection) {
        sortProps.sortDirection = initialSortDirection;
      }

      dispatchSetInteractiveImportSort(sortProps);
    }

    dispatchFetchInteractiveImportItems({
      downloadId,
      seriesId,
      seasonNumber,
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
        seriesId,
        folder
      } = this.props;

      this.props.dispatchFetchInteractiveImportItems({
        downloadId,
        seriesId,
        folder,
        filterExistingFiles
      });
    }
  }

  componentWillUnmount() {
    this.props.dispatchClearInteractiveImport();
  }

  //
  // Listeners

  onSortPress = (sortKey, sortDirection) => {
    this.props.dispatchSetInteractiveImportSort({ sortKey, sortDirection });
  };

  onFilterExistingFilesChange = (filterExistingFiles) => {
    this.setState({ filterExistingFiles });
  };

  onImportModeChange = (importMode) => {
    this.props.dispatchSetInteractiveImportMode({ importMode });
  };

  onDeleteSelectedPress = (selected) => {
    const {
      items,
      dispatchDeleteEpisodeFiles
    } = this.props;

    const episodeFileIds = items.reduce((acc, item) => {
      if (selected.indexOf(item.id) > -1 && item.episodeFileId) {
        acc.push(item.episodeFileId);
      }

      return acc;
    }, []);

    dispatchDeleteEpisodeFiles({ episodeFileIds });
  };

  onImportSelectedPress = (selected, importMode) => {
    const {
      items,
      originalItems,
      dispatchUpdateEpisodeFiles,
      dispatchExecuteCommand,
      onModalClose
    } = this.props;

    const existingFiles = [];
    const files = [];

    if (importMode === 'chooseImportMode') {
      this.setState({ interactiveImportErrorMessage: 'An import mode must be selected' });
      return;
    }

    items.forEach((item) => {
      const isSelected = selected.indexOf(item.id) > -1;

      if (isSelected) {
        const {
          series,
          seasonNumber,
          episodes,
          releaseGroup,
          quality,
          language,
          episodeFileId
        } = item;

        if (!series) {
          this.setState({ interactiveImportErrorMessage: 'Series must be chosen for each selected file' });
          return;
        }

        if (isNaN(seasonNumber)) {
          this.setState({ interactiveImportErrorMessage: 'Season must be chosen for each selected file' });
          return;
        }

        if (!episodes || !episodes.length) {
          this.setState({ interactiveImportErrorMessage: 'One or more episodes must be chosen for each selected file' });
          return;
        }

        if (!quality) {
          this.setState({ interactiveImportErrorMessage: 'Quality must be chosen for each selected file' });
          return;
        }

        if (!language) {
          this.setState({ interactiveImportErrorMessage: 'Language must be chosen for each selected file' });
          return;
        }

        if (episodeFileId) {
          const originalItem = originalItems.find((i) => i.id === item.id);

          if (isSameEpisodeFile(item, originalItem)) {
            existingFiles.push({
              id: episodeFileId,
              releaseGroup,
              quality,
              language
            });

            return;
          }
        }

        files.push({
          path: item.path,
          folderName: item.folderName,
          seriesId: series.id,
          episodeIds: episodes.map((e) => e.id),
          releaseGroup,
          quality,
          language,
          downloadId: this.props.downloadId,
          episodeFileId
        });
      }
    });

    let shouldClose = false;

    if (existingFiles.length) {
      dispatchUpdateEpisodeFiles({
        files: existingFiles
      });

      shouldClose = true;
    }

    if (files.length) {
      dispatchExecuteCommand({
        name: commandNames.INTERACTIVE_IMPORT,
        files,
        importMode
      });

      shouldClose = true;
    }

    if (shouldClose) {
      onModalClose();
    }
  };

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
        onDeleteSelectedPress={this.onDeleteSelectedPress}
        onImportSelectedPress={this.onImportSelectedPress}
      />
    );
  }
}

InteractiveImportModalContentConnector.propTypes = {
  downloadId: PropTypes.string,
  seriesId: PropTypes.number,
  seasonNumber: PropTypes.number,
  folder: PropTypes.string,
  filterExistingFiles: PropTypes.bool.isRequired,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  initialSortKey: PropTypes.string,
  initialSortDirection: PropTypes.oneOf(sortDirections.all),
  originalItems: PropTypes.arrayOf(PropTypes.object).isRequired,
  dispatchFetchInteractiveImportItems: PropTypes.func.isRequired,
  dispatchSetInteractiveImportSort: PropTypes.func.isRequired,
  dispatchSetInteractiveImportMode: PropTypes.func.isRequired,
  dispatchClearInteractiveImport: PropTypes.func.isRequired,
  dispatchUpdateEpisodeFiles: PropTypes.func.isRequired,
  dispatchDeleteEpisodeFiles: PropTypes.func.isRequired,
  dispatchExecuteCommand: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

InteractiveImportModalContentConnector.defaultProps = {
  filterExistingFiles: true
};

export default connect(createMapStateToProps, mapDispatchToProps)(InteractiveImportModalContentConnector);
