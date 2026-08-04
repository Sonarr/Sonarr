import * as dark from './dark';
import * as light from './light';
import * as oled from './oled';

export const themeNames = ['auto', 'light', 'dark', 'oled'];
export const themeStorageKey = 'sonarr.ui.theme';

const browserThemeColors = {
  light: '#3a3f51',
  dark: '#2a2a2a',
  oled: '#000000'
};

function prefersDarkMode() {
  return typeof window !== 'undefined' &&
    typeof window.matchMedia === 'function' &&
    window.matchMedia('(prefers-color-scheme: dark)').matches;
}

export function isThemeName(theme) {
  return themeNames.includes(theme);
}

export function getStoredTheme() {
  if (typeof window === 'undefined') {
    return null;
  }

  try {
    const storedTheme = window.localStorage.getItem(themeStorageKey);

    return isThemeName(storedTheme) ? storedTheme : null;
  } catch {
    return null;
  }
}

export function getResolvedThemeName(theme) {
  if (theme === 'oled') {
    return 'oled';
  }

  if (theme === 'dark' || (theme === 'auto' && prefersDarkMode())) {
    return 'dark';
  }

  return 'light';
}

export function getTheme(theme) {
  return themes[getResolvedThemeName(theme)];
}

export function applyTheme(theme, { persist = true } = {}) {
  if (typeof document === 'undefined') {
    return;
  }

  const selectedTheme = isThemeName(theme) ? theme : 'auto';
  const resolvedTheme = getResolvedThemeName(selectedTheme);
  const themeVariables = themes[resolvedTheme];
  const documentElement = document.documentElement;

  Object.entries(themeVariables).forEach(([key, value]) => {
    documentElement.style.setProperty(`--${key}`, value);
  });

  documentElement.dataset.theme = selectedTheme;
  documentElement.dataset.resolvedTheme = resolvedTheme;
  documentElement.style.backgroundColor = themeVariables.pageBackground;
  documentElement.style.colorScheme = resolvedTheme === 'light' ? 'light' : 'dark';

  const themeColor = browserThemeColors[resolvedTheme];

  document
    .querySelectorAll('meta[name="theme-color"], meta[name="msapplication-navbutton-color"]')
    .forEach((element) => element.setAttribute('content', themeColor));

  if (persist && typeof window !== 'undefined') {
    try {
      window.localStorage.setItem(themeStorageKey, selectedTheme);
    } catch {
      // Local storage can be unavailable in private or restricted contexts.
    }
  }

  if (typeof window !== 'undefined') {
    window.dispatchEvent(new CustomEvent('sonarr:theme-changed', {
      detail: {
        selectedTheme,
        resolvedTheme
      }
    }));
  }
}

const themes = {
  get auto() {
    return prefersDarkMode() ? dark : light;
  },
  light,
  dark,
  oled
};

export default themes;
