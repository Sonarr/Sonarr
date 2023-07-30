import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';

function createTagDetailsSelector() {
  return createSelector(
    (_: AppState, { id }: { id: number }) => id,
    (state: AppState) => state.tags.details.items,
    (id, tagDetails) => {
      return tagDetails.find((t) => t.id === id);
    }
  );
}

export default createTagDetailsSelector;
