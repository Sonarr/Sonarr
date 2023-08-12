import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';

function createQueueItemSelector() {
  return createSelector(
    (_: AppState, { episodeId }: { episodeId: number }) => episodeId,
    (state: AppState) => state.queue.details.items,
    (episodeId, details) => {
      if (!episodeId || !details) {
        return null;
      }

      return details.find((item) => item.episodeId === episodeId);
    }
  );
}

export default createQueueItemSelector;
