import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { deleteEpisodeFile, fetchEpisodeFile } from 'Store/Actions/episodeFileActions';
import createEpisodeFileSelector from 'Store/Selectors/createEpisodeFileSelector';
import createEpisodeSelector from 'Store/Selectors/createEpisodeSelector';
import createSeriesSelector from 'Store/Selectors/createSeriesSelector';
import EpisodeSummary from './EpisodeSummary';

function createMapStateToProps() {
  return createSelector(
    createSeriesSelector(),
    createEpisodeSelector(),
    createEpisodeFileSelector(),
    (series, episode, episodeFile = {}) => {
      const {
        qualityProfileId,
        network
      } = series;

      const {
        airDateUtc,
        overview
      } = episode;

      const {
        mediaInfo,
        path,
        size,
        language,
        languageCutoffNotMet,
        quality,
        qualityCutoffNotMet
      } = episodeFile;

      return {
        network,
        qualityProfileId,
        airDateUtc,
        overview,
        mediaInfo,
        path,
        size,
        language,
        languageCutoffNotMet,
        quality,
        qualityCutoffNotMet
      };
    }
  );
}

function createMapDispatchToProps(dispatch, props) {
  return {
    onDeleteEpisodeFile() {
      dispatch(deleteEpisodeFile({
        id: props.episodeFileId,
        episodeEntity: props.episodeEntity
      }));
    },

    dispatchFetchEpisodeFile() {
      dispatch(fetchEpisodeFile({
        id: props.episodeFileId
      }));
    }
  };
}

class EpisodeSummaryConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    const {
      episodeFileId,
      path,
      dispatchFetchEpisodeFile
    } = this.props;

    if (episodeFileId && !path) {
      dispatchFetchEpisodeFile({ id: episodeFileId });
    }
  }

  //
  // Render

  render() {
    const {
      dispatchFetchEpisodeFile,
      ...otherProps
    } = this.props;

    return <EpisodeSummary {...otherProps} />;
  }
}

EpisodeSummaryConnector.propTypes = {
  episodeFileId: PropTypes.number,
  path: PropTypes.string,
  dispatchFetchEpisodeFile: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, createMapDispatchToProps)(EpisodeSummaryConnector);
