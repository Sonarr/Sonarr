import translate from 'Utilities/String/translate';

function getMediaLanguageNames(
  languages: (string | undefined)[],
  getLanguageName: (code: string) => string
) {
  const names = languages.map((language) => {
    const simpleLanguage = language?.split('_')[0];

    if (!simpleLanguage || simpleLanguage === 'und') {
      return translate('Unknown');
    }

    return getLanguageName(simpleLanguage);
  });

  return [...new Set(names)];
}

export default getMediaLanguageNames;
