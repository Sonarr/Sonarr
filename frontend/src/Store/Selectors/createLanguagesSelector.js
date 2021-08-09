import { createSelector } from 'reselect';

function createLanguagesSelector() {
  return createSelector(
    (state) => state.settings.languageProfiles,
    (languageProfiles) => {
      const {
        isSchemaFetching: isFetching,
        isSchemaPopulated: isPopulated,
        schemaError: error,
        schema
      } = languageProfiles;

      return {
        isFetching,
        isPopulated,
        error,
        items: schema.languages ? [...schema.languages].reverse() : []
      };
    }
  );
}

export default createLanguagesSelector;
