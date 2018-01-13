import _ from 'lodash';
import { createSelector } from 'reselect';

function createLanguageProfileSelector() {
  return createSelector(
    (state, { languageProfileId }) => languageProfileId,
    (state) => state.settings.languageProfiles.items,
    (languageProfileId, languageProfiles) => {
      return _.find(languageProfiles, { id: languageProfileId });
    }
  );
}

export default createLanguageProfileSelector;
