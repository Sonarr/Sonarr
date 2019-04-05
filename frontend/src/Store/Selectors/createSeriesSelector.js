import { createSelector } from 'reselect';
import createAllSeriesSelector from './createAllSeriesSelector';

function createSeriesSelector() {
  return createSelector(
    (state, { seriesId }) => seriesId,
    createAllSeriesSelector(),
    (seriesId, allSeries) => {
      return allSeries.find((series) => series.id === seriesId);
    }
  );
}

export default createSeriesSelector;
