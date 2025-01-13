import { createSelector } from 'reselect';
import createAllSeriesSelector from './createAllSeriesSelector';

function createExistingSeriesSelector(tvdbId: number | undefined) {
  return createSelector(createAllSeriesSelector(), (series) => {
    if (tvdbId == null) {
      return false;
    }

    return series.some((s) => s.tvdbId === tvdbId);
  });
}

export default createExistingSeriesSelector;
