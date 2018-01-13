import _ from 'lodash';
import { createSelector } from 'reselect';
import createAllSeriesSelector from './createAllSeriesSelector';

function createExistingSeriesSelector() {
  return createSelector(
    (state, { tvdbId }) => tvdbId,
    createAllSeriesSelector(),
    (tvdbId, series) => {
      return _.some(series, { tvdbId });
    }
  );
}

export default createExistingSeriesSelector;
