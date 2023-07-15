import createAjaxRequest from 'Utilities/createAjaxRequest';

function getTranslations() {
  return createAjaxRequest({
    global: false,
    dataType: 'json',
    url: '/localization'
  }).request;
}

let translations = {};

export function fetchTranslations() {
  return new Promise(async(resolve) => {
    try {
      const data = await getTranslations();
      translations = data.strings;

      resolve(true);
    } catch (error) {
      resolve(false);
    }
  });
}

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
