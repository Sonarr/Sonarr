import { createSelector } from 'reselect';
import createSeriesSelector from './createSeriesSelector';

function createSeriesQualityProfileSelector() {
  return createSelector(
    (state) => state.settings.qualityProfiles.items,
    createSeriesSelector(),
    (qualityProfiles, series = {}) => {
      return qualityProfiles.find((profile) => {
        return profile.id === series.qualityProfileId;
      });
    }
  );
}

export default createSeriesQualityProfileSelector;
