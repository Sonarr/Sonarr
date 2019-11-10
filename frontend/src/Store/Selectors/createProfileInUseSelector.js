import { createSelector } from 'reselect';
import createAllSeriesSelector from './createAllSeriesSelector';

function createProfileInUseSelector(profileProp) {
  return createSelector(
    (state, { id }) => id,
    createAllSeriesSelector(),
    (state) => state.settings.importLists.items,
    (id, series, lists) => {
      if (!id) {
        return false;
      }

      return series.some((s) => s[profileProp] === id) || lists.some((list) => list[profileProp] === id);
    }
  );
}

export default createProfileInUseSelector;
