import { createSelector } from 'reselect';
import createAllSeriesSelector from './createAllSeriesSelector';

function createSeriesCountSelector() {
  return createSelector(
    createAllSeriesSelector(),
    (series) => {
      return series.length;
    }
  );
}

export default createSeriesCountSelector;
