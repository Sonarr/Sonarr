import { createSelector } from 'reselect';
import { LanguageSettingsAppState } from 'App/State/SettingsAppState';
import Language from 'Language/Language';
import createLanguagesSelector from './createLanguagesSelector';

export default function createFilteredLanguagesSelector(filterUnknown = false) {
  const filterItems = ['Any', 'Original'];

  if (filterUnknown) {
    filterItems.push('Unknown');
  }

  return createSelector(createLanguagesSelector(), (languages) => {
    const { isFetching, isPopulated, error, items } =
      languages as LanguageSettingsAppState;

    const filteredLanguages = items.filter(
      (lang: Language) => !filterItems.includes(lang.name)
    );

    return {
      isFetching,
      isPopulated,
      error,
      items: filteredLanguages,
    };
  });
}
