import moment from 'moment-timezone';

async function loadMomentLocale(identifier: string | undefined) {
  if (!identifier || identifier === 'en') {
    return;
  }

  try {
    // All locales are bundled in one chunk that is only fetched when needed
    await import('./momentLocales');

    moment.locale([identifier, 'en']);
  } catch (e) {
    console.error(`Failed to load moment locale '${identifier}'`, e);
  }
}

export default loadMomentLocale;
