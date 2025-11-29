import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';
import Series from 'Series/Series';
import QualityProfile from 'typings/QualityProfile';

function createSeriesQualityProfileSelector(series?: Series) {
  return createSelector(
    (state: AppState) => state.settings.qualityProfiles.items,
    (qualityProfiles: QualityProfile[]) => {
      if (!series) {
        return undefined;
      }

      return qualityProfiles.find(
        (profile) => profile.id === series.qualityProfileId
      );
    }
  );
}

export default createSeriesQualityProfileSelector;
