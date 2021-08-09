import createAjaxRequest from 'Utilities/createAjaxRequest';

function getTranslations() {
  return createAjaxRequest({
    global: false,
    dataType: 'json',
    url: '/localization'
  }).request;
}

let translations = {};
getTranslations().then((data) => {
  translations = data;
});

export default function translate(key, args = '') {
  if (args) {
    const translatedKey = translate(key);
    return translatedKey.replace(/\{(\w+)\}/g, (match, index) => {
      return args[index];
    });
  }

  return translations[key] || key;
}
