import { createSelector } from 'reselect';

export function createQualityProfileSelectorForHook(qualityProfileId) {
  return createSelector(
    (state) => state.settings.qualityProfiles.items,
    (qualityProfiles) => {
      return qualityProfiles.find((profile) => {
        return profile.id === qualityProfileId;
      });
    }
  );
}

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
