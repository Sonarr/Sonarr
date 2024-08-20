import { useSelector } from 'react-redux';
import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';

function createEpisodeFileSelector(episodeFileId?: number) {
  return createSelector(
    (state: AppState) => state.episodeFiles.items,
    (episodeFiles) => {
      return episodeFiles.find(({ id }) => id === episodeFileId);
    }
  );
}

function useEpisodeFile(episodeFileId: number | undefined) {
  return useSelector(createEpisodeFileSelector(episodeFileId));
}

export default useEpisodeFile;
