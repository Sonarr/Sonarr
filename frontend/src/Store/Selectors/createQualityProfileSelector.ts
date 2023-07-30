import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';

export function createQualityProfileSelectorForHook(qualityProfileId: number) {
  return createSelector(
    (state: AppState) => state.settings.qualityProfiles.items,
    (qualityProfiles) => {
      return qualityProfiles.find((profile) => profile.id === qualityProfileId);
    }
  );
}

function createQualityProfileSelector() {
  return createSelector(
    (_: AppState, { qualityProfileId }: { qualityProfileId: number }) =>
      qualityProfileId,
    (state: AppState) => state.settings.qualityProfiles.items,
    (qualityProfileId, qualityProfiles) => {
      return qualityProfiles.find((profile) => profile.id === qualityProfileId);
    }
  );
}

export default createQualityProfileSelector;
