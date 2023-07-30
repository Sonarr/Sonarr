import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';

function createEpisodeFileSelector() {
  return createSelector(
    (_: AppState, { episodeFileId }: { episodeFileId: number }) =>
      episodeFileId,
    (state: AppState) => state.episodeFiles,
    (episodeFileId, episodeFiles) => {
      if (!episodeFileId) {
        return;
      }

      return episodeFiles.items.find(
        (episodeFile) => episodeFile.id === episodeFileId
      );
    }
  );
}

export default createEpisodeFileSelector;
