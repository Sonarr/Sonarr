import _ from 'lodash';
import { createSelector } from 'reselect';

function createQueueItemSelector() {
  return createSelector(
    (state, { episodeId }) => episodeId,
    (state) => state.queue.details,
    (episodeId, details) => {
      if (!episodeId) {
        return null;
      }

      return _.find(details.items, (item) => {
        return item.episode.id === episodeId;
      });
    }
  );
}

export default createQueueItemSelector;
