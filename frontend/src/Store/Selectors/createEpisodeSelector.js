import _ from 'lodash';
import { createSelector } from 'reselect';
import episodeEntities from 'Episode/episodeEntities';

function createEpisodeSelector() {
  return createSelector(
    (state, { episodeId }) => episodeId,
    (state, { episodeEntity = episodeEntities.EPISODES }) => _.get(state, episodeEntity, { items: [] }),
    (episodeId, episodes) => {
      return _.find(episodes.items, { id: episodeId });
    }
  );
}

export default createEpisodeSelector;
