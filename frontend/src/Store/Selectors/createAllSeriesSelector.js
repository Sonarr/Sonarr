import { createSelector } from 'reselect';

function createAllSeriesSelector() {
  return createSelector(
    (state) => state.series,
    (series) => {
      return series.items;
    }
  );
}

export default createAllSeriesSelector;
