import { createSelector } from 'reselect';

export function createSeriesSelectorForHook(seriesId) {
  return createSelector(
    (state) => state.series.itemMap,
    (state) => state.series.items,
    (itemMap, allSeries) => {
      return seriesId ? allSeries[itemMap[seriesId]]: undefined;
    }
  );
}

function createSeriesSelector() {
  return createSelector(
    (state, { seriesId }) => seriesId,
    (state) => state.series.itemMap,
    (state) => state.series.items,
    (seriesId, itemMap, allSeries) => {
      return allSeries[itemMap[seriesId]];
    }
  );
}

export default createSeriesSelector;
