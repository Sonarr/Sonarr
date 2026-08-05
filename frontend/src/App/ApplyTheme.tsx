import { useEffect } from 'react';
import { useUiSettingsValues } from 'Settings/UI/useUiSettings';
import { applyTheme, isThemeName, themeStorageKey } from 'Styles/Themes';

function ApplyTheme() {
  const configuredTheme = useUiSettingsValues().theme ?? window.Sonarr.theme;
  const selectedTheme = isThemeName(configuredTheme) ? configuredTheme : 'auto';

  useEffect(() => {
    applyTheme(selectedTheme);

    const colorSchemeQuery = window.matchMedia('(prefers-color-scheme: dark)');

    const handleColorSchemeChange = () => {
      if (selectedTheme === 'auto') {
        applyTheme(selectedTheme, { persist: false });
      }
    };

    const handleStorageChange = (event: StorageEvent) => {
      if (event.key !== themeStorageKey) {
        return;
      }

      const storedTheme = isThemeName(event.newValue) ? event.newValue : 'auto';

      if (storedTheme !== selectedTheme) {
        window.location.reload();
      }
    };

    if (colorSchemeQuery.addEventListener) {
      colorSchemeQuery.addEventListener('change', handleColorSchemeChange);
    } else {
      colorSchemeQuery.addListener(handleColorSchemeChange);
    }

    window.addEventListener('storage', handleStorageChange);

    return () => {
      if (colorSchemeQuery.removeEventListener) {
        colorSchemeQuery.removeEventListener('change', handleColorSchemeChange);
      } else {
        colorSchemeQuery.removeListener(handleColorSchemeChange);
      }

      window.removeEventListener('storage', handleStorageChange);
    };
  }, [selectedTheme]);

  return null;
}

export default ApplyTheme;
