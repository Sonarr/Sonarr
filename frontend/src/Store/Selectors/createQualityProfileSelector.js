import _ from 'lodash';
import { createSelector } from 'reselect';

function createQualityProfileSelector() {
  return createSelector(
    (state, { qualityProfileId }) => qualityProfileId,
    (state) => state.settings.qualityProfiles.items,
    (qualityProfileId, qualityProfiles) => {
      return _.find(qualityProfiles, { id: qualityProfileId });
    }
  );
}

export default createQualityProfileSelector;
