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
  translations = data.strings;
});

export default function translate(key, tokens) {
  const translation = translations[key] || key;

  if (tokens) {
    return translation.replace(
      /\{([a-z0-9]+?)\}/gi,
      (match, tokenMatch) => String(tokens[tokenMatch]) ?? match
    );
  }

  return translation;
}
