import { createSelector } from 'reselect';

function createTagsSelector() {
  return createSelector(
    (state) => state.tags.items,
    (tags) => {
      return tags;
    }
  );
}

export default createTagsSelector;
