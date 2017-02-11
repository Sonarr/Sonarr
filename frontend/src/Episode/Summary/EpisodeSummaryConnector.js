import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { deleteEpisodeFile } from 'Store/Actions/episodeFileActions';
import createEpisodeSelector from 'Store/Selectors/createEpisodeSelector';
import createEpisodeFileSelector from 'Store/Selectors/createEpisodeFileSelector';
import createSeriesSelector from 'Store/Selectors/createSeriesSelector';
import EpisodeSummary from './EpisodeSummary';

function createMapStateToProps() {
  return createSelector(
    createSeriesSelector(),
    createEpisodeSelector(),
    createEpisodeFileSelector(),
    (series, episode, episodeFile) => {
      const {
        qualityProfileId,
        network
      } = series;

      const {
        airDateUtc,
        overview
      } = episode;

      const {
        path,
        size,
        quality,
        qualityCutoffNotMet
      } = episodeFile || {};

      return {
        network,
        qualityProfileId,
        airDateUtc,
        overview,
        path,
        size,
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
    }
  };
}

export default connect(createMapStateToProps, createMapDispatchToProps)(EpisodeSummary);
