import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import {
  clearInteractiveImportEpisodes,
  fetchInteractiveImportEpisodes,
  reprocessInteractiveImportItems,
  setInteractiveImportEpisodesSort,
  updateInteractiveImportItem } from 'Store/Actions/interactiveImportActions';
import createClientSideCollectionSelector from 'Store/Selectors/createClientSideCollectionSelector';
import SelectEpisodeModalContent from './SelectEpisodeModalContent';

function createMapStateToProps() {
  return createSelector(
    createClientSideCollectionSelector('interactiveImport.episodes'),
    (episodes) => {
      return episodes;
    }
  );
}

const mapDispatchToProps = {
  dispatchFetchInteractiveImportEpisodes: fetchInteractiveImportEpisodes,
  dispatchSetInteractiveImportEpisodesSort: setInteractiveImportEpisodesSort,
  dispatchClearInteractiveImportEpisodes: clearInteractiveImportEpisodes,
  dispatchUpdateInteractiveImportItem: updateInteractiveImportItem,
  dispatchReprocessInteractiveImportItems: reprocessInteractiveImportItems
};

class SelectEpisodeModalContentConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    const {
      seriesId,
      seasonNumber
    } = this.props;

    this.props.dispatchFetchInteractiveImportEpisodes({ seriesId, seasonNumber });
  }

  componentWillUnmount() {
    // This clears the episodes for the queue and hides the queue
    // We'll need another place to store episodes for manual import
    this.props.dispatchClearInteractiveImportEpisodes();
  }

  //
  // Listeners

  onSortPress = (sortKey, sortDirection) => {
    this.props.dispatchSetInteractiveImportEpisodesSort({ sortKey, sortDirection });
  };

  onEpisodesSelect = (episodeIds) => {
    const {
      ids,
      items,
      dispatchUpdateInteractiveImportItem,
      dispatchReprocessInteractiveImportItems,
      onModalClose
    } = this.props;

    const selectedEpisodes = items.reduce((acc, item) => {
      if (episodeIds.indexOf(item.id) > -1) {
        acc.push(item);
      }

      return acc;
    }, []);

    const episodesPerFile = selectedEpisodes.length / ids.length;
    const sortedEpisodes = selectedEpisodes.sort((a, b) => {
      return a.seasonNumber - b.seasonNumber;
    });

    ids.forEach((id, index) => {
      const startingIndex = index * episodesPerFile;
      const episodes = sortedEpisodes.slice(startingIndex, startingIndex + episodesPerFile);

      dispatchUpdateInteractiveImportItem({
        id,
        episodes
      });
    });

    dispatchReprocessInteractiveImportItems({ ids });

    onModalClose(true);
  };

  //
  // Render

  render() {
    return (
      <SelectEpisodeModalContent
        {...this.props}
        onSortPress={this.onSortPress}
        onEpisodesSelect={this.onEpisodesSelect}
      />
    );
  }
}

SelectEpisodeModalContentConnector.propTypes = {
  ids: PropTypes.arrayOf(PropTypes.number).isRequired,
  seriesId: PropTypes.number.isRequired,
  seasonNumber: PropTypes.number.isRequired,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  dispatchFetchInteractiveImportEpisodes: PropTypes.func.isRequired,
  dispatchSetInteractiveImportEpisodesSort: PropTypes.func.isRequired,
  dispatchClearInteractiveImportEpisodes: PropTypes.func.isRequired,
  dispatchUpdateInteractiveImportItem: PropTypes.func.isRequired,
  dispatchReprocessInteractiveImportItems: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(SelectEpisodeModalContentConnector);
