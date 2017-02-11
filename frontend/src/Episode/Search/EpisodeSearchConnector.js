import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { executeCommand } from 'Store/Actions/commandActions';
import * as commandNames from 'Commands/commandNames';
import EpisodeSearch from './EpisodeSearch';
import InteractiveEpisodeSearchConnector from './InteractiveEpisodeSearchConnector';

function createMapStateToProps() {
  return createSelector(
    (state) => state.releases,
    (releases) => {
      return {
        isPopulated: releases.isPopulated
      };
    }
  );
}

const mapDispatchToProps = {
  executeCommand
};

class EpisodeSearchConnector extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isInteractiveSearchOpen: props.startInteractiveSearch
    };
  }

  componentDidMount() {
    if (this.props.isPopulated) {
      this.setState({ isInteractiveSearchOpen: true });
    }
  }

  //
  // Listeners

  onQuickSearchPress = () => {
    this.props.executeCommand({
      name: commandNames.EPISODE_SEARCH,
      episodeIds: [this.props.episodeId]
    });

    this.props.onModalClose();
  }

  onInteractiveSearchPress = () => {
    this.setState({ isInteractiveSearchOpen: true });
  }

  //
  // Render

  render() {
    if (this.state.isInteractiveSearchOpen) {
      return (
        <InteractiveEpisodeSearchConnector
          {...this.props}
        />
      );
    }

    return (
      <EpisodeSearch
        {...this.props}
        onQuickSearchPress={this.onQuickSearchPress}
        onInteractiveSearchPress={this.onInteractiveSearchPress}
      />
    );
  }
}

EpisodeSearchConnector.propTypes = {
  episodeId: PropTypes.number.isRequired,
  isPopulated: PropTypes.bool.isRequired,
  startInteractiveSearch: PropTypes.bool.isRequired,
  onModalClose: PropTypes.func.isRequired,
  executeCommand: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(EpisodeSearchConnector);
