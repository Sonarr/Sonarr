import _ from 'lodash';
import { createSelector } from 'reselect';
import createAllSeriesSelector from './createAllSeriesSelector';

function createSeriesSelector() {
  return createSelector(
    (state, { seriesId }) => seriesId,
    createAllSeriesSelector(),
    (seriesId, series) => {
      return _.find(series, { id: seriesId });
    }
  );
}

export default createSeriesSelector;
