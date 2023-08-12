import { some } from 'lodash';
import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';
import createAllSeriesSelector from './createAllSeriesSelector';

function createExistingSeriesSelector() {
  return createSelector(
    (_: AppState, { tvdbId }: { tvdbId: number }) => tvdbId,
    createAllSeriesSelector(),
    (tvdbId, series) => {
      return some(series, { tvdbId });
    }
  );
}

export default createExistingSeriesSelector;
