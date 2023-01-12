import createAjaxRequest from 'Utilities/createAjaxRequest';

function getLanguage() {
  return createAjaxRequest({
    global: false,
    dataType: 'json',
    url: '/localization/language'
  }).request;
}

function getDisplayName(code) {
  return Intl.DisplayNames ?
    new Intl.DisplayNames([code], { type: 'language' }) :
    null;
}

let languageNames = getDisplayName('en');

getLanguage().then((data) => {
  const names = getDisplayName(data.identifier);

  if (names) {
    languageNames = names;
  }
});

export default function getLanguageName(code) {
  if (!languageNames) {
    return code;
  }

  return languageNames.of(code) ?? code;
}
