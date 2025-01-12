import { some } from 'lodash';
import { createSelector } from 'reselect';
import createAllSeriesSelector from './createAllSeriesSelector';

function createExistingSeriesSelector(tvdbId: number) {
  return createSelector(createAllSeriesSelector(), (series) => {
    return some(series, { tvdbId });
  });
}

export default createExistingSeriesSelector;
