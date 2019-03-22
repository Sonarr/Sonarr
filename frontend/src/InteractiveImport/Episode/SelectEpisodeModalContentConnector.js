import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import {
  updateInteractiveImportItem,
  fetchInteractiveImportEpisodes,
  setInteractiveImportEpisodesSort,
  clearInteractiveImportEpisodes
} from 'Store/Actions/interactiveImportActions';
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
  fetchInteractiveImportEpisodes,
  setInteractiveImportEpisodesSort,
  clearInteractiveImportEpisodes,
  updateInteractiveImportItem
};

class SelectEpisodeModalContentConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    const {
      seriesId,
      seasonNumber
    } = this.props;

    this.props.fetchInteractiveImportEpisodes({ seriesId, seasonNumber });
  }

  componentWillUnmount() {
    // This clears the episodes for the queue and hides the queue
    // We'll need another place to store episodes for manual import
    this.props.clearInteractiveImportEpisodes();
  }

  //
  // Listeners

  onSortPress = (sortKey, sortDirection) => {
    this.props.setInteractiveImportEpisodesSort({ sortKey, sortDirection });
  }

  onEpisodesSelect = (episodeIds) => {
    const episodes = _.reduce(this.props.items, (acc, item) => {
      if (episodeIds.indexOf(item.id) > -1) {
        acc.push(item);
      }

      return acc;
    }, []);

    this.props.updateInteractiveImportItem({
      id: this.props.id,
      episodes: _.sortBy(episodes, 'episodeNumber')
    });

    this.props.onModalClose(true);
  }

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
  id: PropTypes.number.isRequired,
  seriesId: PropTypes.number.isRequired,
  seasonNumber: PropTypes.number.isRequired,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  fetchInteractiveImportEpisodes: PropTypes.func.isRequired,
  setInteractiveImportEpisodesSort: PropTypes.func.isRequired,
  clearInteractiveImportEpisodes: PropTypes.func.isRequired,
  updateInteractiveImportItem: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(SelectEpisodeModalContentConnector);
