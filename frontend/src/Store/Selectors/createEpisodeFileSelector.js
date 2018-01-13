import { createSelector } from 'reselect';

function createEpisodeFileSelector() {
  return createSelector(
    (state, { episodeFileId }) => episodeFileId,
    (state) => state.episodeFiles,
    (episodeFileId, episodeFiles) => {
      if (!episodeFileId) {
        return null;
      }

      return episodeFiles.items.find((episodeFile) => episodeFile.id === episodeFileId);
    }
  );
}

export default createEpisodeFileSelector;
