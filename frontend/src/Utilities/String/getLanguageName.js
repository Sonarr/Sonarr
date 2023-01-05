import createAjaxRequest from 'Utilities/createAjaxRequest';

function getTranslations() {
  return createAjaxRequest({
    global: false,
    dataType: 'json',
    url: '/localization/language'
  }).request;
}

let languageNames = new Intl.DisplayNames(['en'], { type: 'language' });

getTranslations().then((data) => {
  const names = new Intl.DisplayNames([data.identifier], { type: 'language' });

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
