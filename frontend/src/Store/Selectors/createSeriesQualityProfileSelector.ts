import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';
import Series from 'Series/Series';
import { createSeriesSelectorForHook } from './createSeriesSelector';

function createSeriesQualityProfileSelector(seriesId: number) {
  return createSelector(
    (state: AppState) => state.settings.qualityProfiles.items,
    createSeriesSelectorForHook(seriesId),
    (qualityProfiles, series = {} as Series) => {
      return qualityProfiles.find(
        (profile) => profile.id === series.qualityProfileId
      );
    }
  );
}

export default createSeriesQualityProfileSelector;
