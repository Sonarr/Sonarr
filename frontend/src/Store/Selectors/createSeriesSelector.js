import { createSelector } from 'reselect';

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
