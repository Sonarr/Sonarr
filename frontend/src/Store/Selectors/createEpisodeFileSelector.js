import _ from 'lodash';
import { createSelector } from 'reselect';

function createEpisodeFileSelector() {
  return createSelector(
    (state, { episodeFileId }) => episodeFileId,
    (state) => state.episodeFiles,
    (episodeFileId, episodeFiles) => {
      if (!episodeFileId) {
        return null;
      }

      return _.find(episodeFiles.items, { id: episodeFileId });
    }
  );
}

export default createEpisodeFileSelector;
