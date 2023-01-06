import { createSelector } from 'reselect';

function createSeriesSelector(id) {
  if (id == null) {
    return createSelector(
      (state, { seriesId }) => seriesId,
      (state) => state.series.itemMap,
      (state) => state.series.items,
      (seriesId, itemMap, allSeries) => {
        return allSeries[itemMap[seriesId]];
      }
    );
  }

  return createSelector(
    (state) => state.series.itemMap,
    (state) => state.series.items,
    (itemMap, allSeries) => {
      return allSeries[itemMap[id]];
    }
  );
}

export default createSeriesSelector;
