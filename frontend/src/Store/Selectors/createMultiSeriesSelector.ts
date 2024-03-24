import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';
import Series from 'Series/Series';

function createMultiSeriesSelector(seriesIds: number[]) {
  return createSelector(
    (state: AppState) => state.series.itemMap,
    (state: AppState) => state.series.items,
    (itemMap, allSeries) => {
      return seriesIds.reduce((acc: Series[], seriesId) => {
        const series = allSeries[itemMap[seriesId]];

        if (series) {
          acc.push(series);
        }

        return acc;
      }, []);
    }
  );
}

export default createMultiSeriesSelector;
