import { createSelector } from 'reselect';
import { createSeriesSelectorForHook } from './createSeriesSelector';

function createSeriesQualityProfileSelector(seriesId) {
  return createSelector(
    (state) => state.settings.qualityProfiles.items,
    createSeriesSelectorForHook(seriesId),
    (qualityProfiles, series = {}) => {
      return qualityProfiles.find((profile) => {
        return profile.id === series.qualityProfileId;
      });
    }
  );
}

export default createSeriesQualityProfileSelector;
