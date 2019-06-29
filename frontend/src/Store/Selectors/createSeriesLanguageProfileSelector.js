import { createSelector } from 'reselect';
import createSeriesSelector from './createSeriesSelector';

function createSeriesLanguageProfileSelector() {
  return createSelector(
    (state) => state.settings.languageProfiles.items,
    createSeriesSelector(),
    (languageProfiles, series = {}) => {
      return languageProfiles.find((profile) => {
        return profile.id === series.languageProfileId;
      });
    }
  );
}

export default createSeriesLanguageProfileSelector;
