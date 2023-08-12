import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';
import Series from 'Series/Series';
import ImportList from 'typings/ImportList';
import createAllSeriesSelector from './createAllSeriesSelector';

function createProfileInUseSelector(profileProp: string) {
  return createSelector(
    (_: AppState, { id }: { id: number }) => id,
    createAllSeriesSelector(),
    (state: AppState) => state.settings.importLists.items,
    (id, series, lists) => {
      if (!id) {
        return false;
      }

      return (
        series.some((s) => s[profileProp as keyof Series] === id) ||
        lists.some((list) => list[profileProp as keyof ImportList] === id)
      );
    }
  );
}

export default createProfileInUseSelector;
