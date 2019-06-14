import { createSelector } from 'reselect';

function createIndexerSelector() {
  return createSelector(
    (state, { indexerId }) => indexerId,
    (state) => state.settings.indexers.items,
    (indexerId, indexers) => {
      return indexers.find((profile) => {
        return profile.id === indexerId;
      });
    }
  );
}

export default createIndexerSelector;
