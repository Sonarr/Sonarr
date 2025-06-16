import { createSelector } from 'reselect';
import Series from 'Series/Series';
import createAllSeriesSelector from './createAllSeriesSelector';

export function isExistingSeries(
  series: Series[],
  tvdbId: number | undefined
): boolean {
  if (tvdbId == null) {
    return false;
  }

  return series.some((s) => s.tvdbId === tvdbId);
}

function createExistingSeriesSelector(tvdbId: number | undefined) {
  return createSelector(createAllSeriesSelector(), (series) => {
    if (tvdbId == null) {
      return false;
    }

    return series.some((s) => s.tvdbId === tvdbId);
  });
}

export default createExistingSeriesSelector;
