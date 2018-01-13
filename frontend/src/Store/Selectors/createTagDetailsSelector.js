import { createSelector } from 'reselect';

function createTagDetailsSelector() {
  return createSelector(
    (state, { id }) => id,
    (state) => state.tags.details.items,
    (id, tagDetails) => {
      return tagDetails.find((t) => t.id === id);
    }
  );
}

export default createTagDetailsSelector;
