import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';

function createMultiSeriesSelector(seriesIds: number[]) {
  return createSelector(
    (state: AppState) => state.series.itemMap,
    (state: AppState) => state.series.items,
    (itemMap, allSeries) => {
      return seriesIds.map((seriesId) => allSeries[itemMap[seriesId]]);
    }
  );
}

export default createMultiSeriesSelector;
