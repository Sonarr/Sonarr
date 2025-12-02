let translations: Record<string, string> = {};

export function setTranslations(translationData: Record<string, string>) {
  translations = translationData;
}

export function translate(
  key: string,
  tokens: Record<string, string | number | boolean> = {}
) {
  const { isProduction = true } = window.Sonarr;

  if (!isProduction && !(key in translations)) {
    console.warn(`Missing translation for key: ${key}`);
  }

  const translation = translations[key] || key;

  tokens.appName = 'Sonarr';

  return translation.replace(/\{([a-z0-9]+?)\}/gi, (match, tokenMatch) =>
    String(tokens[tokenMatch] ?? match)
  );
}

export default translate;
