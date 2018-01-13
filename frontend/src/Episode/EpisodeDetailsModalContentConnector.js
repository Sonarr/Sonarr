import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { cancelFetchReleases, clearReleases } from 'Store/Actions/releaseActions';
import { toggleEpisodeMonitored } from 'Store/Actions/episodeActions';
import createEpisodeSelector from 'Store/Selectors/createEpisodeSelector';
import createSeriesSelector from 'Store/Selectors/createSeriesSelector';
import episodeEntities from 'Episode/episodeEntities';
import EpisodeDetailsModalContent from './EpisodeDetailsModalContent';

function createMapStateToProps() {
  return createSelector(
    createEpisodeSelector(),
    createSeriesSelector(),
    (episode, series) => {
      const {
        title: seriesTitle,
        titleSlug,
        monitored: seriesMonitored,
        seriesType
      } = series;

      return {
        seriesTitle,
        titleSlug,
        seriesMonitored,
        seriesType,
        ...episode
      };
    }
  );
}

function createMapDispatchToProps(dispatch, props) {
  return {
    dispatchCancelFetchReleases() {
      dispatch(cancelFetchReleases());
    },

    dispatchClearReleases() {
      dispatch(clearReleases());
    },

    onMonitorEpisodePress(monitored) {
      const {
        episodeId,
        episodeEntity
      } = props;

      dispatch(toggleEpisodeMonitored({
        episodeEntity,
        episodeId,
        monitored
      }));
    }
  };
}

class EpisodeDetailsModalContentConnector extends Component {

  //
  // Lifecycle

  componentWillUnmount() {
    // Clear pending releases here so we can reshow the search
    // results even after switching tabs.

    this.props.dispatchCancelFetchReleases();
    this.props.dispatchClearReleases();
  }

  //
  // Render

  render() {
    const {
      dispatchClearReleases,
      ...otherProps
    } = this.props;

    return (
      <EpisodeDetailsModalContent {...otherProps} />
    );
  }
}

EpisodeDetailsModalContentConnector.propTypes = {
  episodeId: PropTypes.number.isRequired,
  episodeEntity: PropTypes.string.isRequired,
  seriesId: PropTypes.number.isRequired,
  dispatchCancelFetchReleases: PropTypes.func.isRequired,
  dispatchClearReleases: PropTypes.func.isRequired
};

EpisodeDetailsModalContentConnector.defaultProps = {
  episodeEntity: episodeEntities.EPISODES
};

export default connect(createMapStateToProps, createMapDispatchToProps)(EpisodeDetailsModalContentConnector);
