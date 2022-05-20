import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import * as commandNames from 'Commands/commandNames';
import { executeCommand } from 'Store/Actions/commandActions';
import createCommandsSelector from 'Store/Selectors/createCommandsSelector';
import createSeriesSelector from 'Store/Selectors/createSeriesSelector';
import { isCommandExecuting } from 'Utilities/Command';
import EpisodeSearchCell from './EpisodeSearchCell';

function createMapStateToProps() {
  return createSelector(
    (state, { episodeId }) => episodeId,
    (state, { sceneSeasonNumber }) => sceneSeasonNumber,
    createSeriesSelector(),
    createCommandsSelector(),
    (episodeId, sceneSeasonNumber, series, commands) => {
      const isSearching = commands.some((command) => {
        const episodeSearch = command.name === commandNames.EPISODE_SEARCH;

        if (!episodeSearch) {
          return false;
        }

        return (
          isCommandExecuting(command) &&
          command.body.episodeIds.indexOf(episodeId) > -1
        );
      });

      return {
        seriesMonitored: series.monitored,
        seriesType: series.seriesType,
        isSearching
      };
    }
  );
}

function createMapDispatchToProps(dispatch, props) {
  return {
    onSearchPress(name, path) {
      dispatch(executeCommand({
        name: commandNames.EPISODE_SEARCH,
        episodeIds: [props.episodeId]
      }));
    }
  };
}

export default connect(createMapStateToProps, createMapDispatchToProps)(EpisodeSearchCell);
