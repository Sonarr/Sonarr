import { createSelector } from 'reselect';

function createQueueItemSelector() {
  return createSelector(
    (state, { episodeId }) => episodeId,
    (state) => state.queue.details.items,
    (episodeId, details) => {
      if (!episodeId || !details) {
        return null;
      }

      return details.find((item) => {
        return item.episodeId === episodeId;
      });
    }
  );
}

export default createQueueItemSelector;
