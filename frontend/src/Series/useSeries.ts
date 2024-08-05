import { useSelector } from 'react-redux';
import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';

export function createSeriesSelector(seriesId?: number) {
  return createSelector(
    (state: AppState) => state.series.itemMap,
    (state: AppState) => state.series.items,
    (itemMap, allSeries) => {
      return seriesId ? allSeries[itemMap[seriesId]] : undefined;
    }
  );
}

function useSeries(seriesId?: number) {
  return useSelector(createSeriesSelector(seriesId));
}

export default useSeries;
