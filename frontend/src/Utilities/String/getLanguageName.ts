import createAjaxRequest from 'Utilities/createAjaxRequest';

interface LanguageResponse {
  identifier: string;
}

function getLanguage() {
  return createAjaxRequest({
    global: false,
    dataType: 'json',
    url: '/localization/language',
  }).request;
}

function getDisplayName(code: string) {
  return Intl.DisplayNames
    ? new Intl.DisplayNames([code], { type: 'language' })
    : null;
}

let languageNames = getDisplayName('en');

getLanguage().then((data: LanguageResponse) => {
  const names = getDisplayName(data.identifier);

  if (names) {
    languageNames = names;
  }
});

export default function getLanguageName(code: string) {
  if (!languageNames) {
    return code;
  }

  try {
    return languageNames.of(code) ?? code;
  } catch (error) {
    return code;
  }
}
