import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';

function createTagsSelector() {
  return createSelector(
    (state: AppState) => state.tags.items,
    (tags) => {
      return tags;
    }
  );
}

export default createTagsSelector;
