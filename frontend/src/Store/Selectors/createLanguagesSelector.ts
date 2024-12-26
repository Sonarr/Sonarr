import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';

interface LanguageFilter {
  [key: string]: boolean | undefined;
  Any: boolean;
  Original?: boolean;
  Unknown?: boolean;
}

function createLanguagesSelector(
  excludeLanguages: LanguageFilter = { Any: true }
) {
  return createSelector(
    (state: AppState) => state.settings.languages,
    (languages) => {
      const { isFetching, isPopulated, error, items } = languages;

      const filteredLanguages = items.filter(
        (lang) => !excludeLanguages[lang.name]
      );

      return {
        isFetching,
        isPopulated,
        error,
        items: filteredLanguages,
      };
    }
  );
}

export default createLanguagesSelector;
