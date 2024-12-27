import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';

function createTagDetailsSelector(id: number) {
  return createSelector(
    (state: AppState) => state.tags.details.items,
    (tagDetails) => {
      return tagDetails.find((t) => t.id === id);
    }
  );
}

export default createTagDetailsSelector;
