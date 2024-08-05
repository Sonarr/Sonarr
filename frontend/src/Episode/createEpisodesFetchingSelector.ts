import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';

function createEpisodesFetchingSelector() {
  return createSelector(
    (state: AppState) => state.episodes,
    (episodes) => {
      return {
        isEpisodesFetching: episodes.isFetching,
        isEpisodesPopulated: episodes.isPopulated,
        episodesError: episodes.error,
      };
    }
  );
}

export default createEpisodesFetchingSelector;
