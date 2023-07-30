import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';

function createAllSeriesSelector() {
  return createSelector(
    (state: AppState) => state.series,
    (series) => {
      return series.items;
    }
  );
}

export default createAllSeriesSelector;
