import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';

function createLanguagesSelector() {
  return createSelector(
    (state: AppState) => state.settings.languages,
    (languages) => {
      const { isFetching, isPopulated, error, items } = languages;

      const filterItems = ['Any'];
      const filteredLanguages = items.filter(
        (lang) => !filterItems.includes(lang.name)
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
