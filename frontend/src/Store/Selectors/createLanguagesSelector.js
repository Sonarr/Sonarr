import { createSelector } from 'reselect';

function createLanguagesSelector() {
  return createSelector(
    (state) => state.settings.languages,
    (languages) => {
      const {
        isFetching,
        isPopulated,
        error,
        items
      } = languages;

      const filterItems = ['Any'];
      const filteredLanguages = items.filter((lang) => !filterItems.includes(lang.name));

      return {
        isFetching,
        isPopulated,
        error,
        items: filteredLanguages
      };
    }
  );
}

export default createLanguagesSelector;
