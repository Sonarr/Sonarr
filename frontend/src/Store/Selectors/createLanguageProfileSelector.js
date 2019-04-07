import { createSelector } from 'reselect';

function createLanguageProfileSelector() {
  return createSelector(
    (state, { languageProfileId }) => languageProfileId,
    (state) => state.settings.languageProfiles.items,
    (languageProfileId, languageProfiles) => {
      return languageProfiles.find((profile) => {
        return profile.id === languageProfileId;
      });
    }
  );
}

export default createLanguageProfileSelector;
