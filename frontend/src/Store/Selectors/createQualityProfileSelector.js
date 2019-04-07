import { createSelector } from 'reselect';

function createQualityProfileSelector() {
  return createSelector(
    (state, { qualityProfileId }) => qualityProfileId,
    (state) => state.settings.qualityProfiles.items,
    (qualityProfileId, qualityProfiles) => {
      return qualityProfiles.find((profile) => {
        return profile.id === qualityProfileId;
      });
    }
  );
}

export default createQualityProfileSelector;
