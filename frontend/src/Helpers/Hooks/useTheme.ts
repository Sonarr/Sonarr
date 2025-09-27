import { useEffect, useState } from 'react';
import { useSelector } from 'react-redux';
import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';
import themes from 'Styles/Themes';

function createThemeSelector() {
  return createSelector(
    (state: AppState) => state.settings.ui.item.theme || window.Sonarr.theme,
    (theme) => theme
  );
}

const useTheme = () => {
  const selectedTheme = useSelector(createThemeSelector());
  const [resolvedTheme, setResolvedTheme] = useState(selectedTheme);

  useEffect(() => {
    if (selectedTheme !== 'auto') {
      setResolvedTheme(selectedTheme);
      return;
    }

    const applySystemTheme = () => {
      setResolvedTheme(
        window.matchMedia('(prefers-color-scheme: dark)').matches
          ? 'dark'
          : 'light'
      );
    };

    applySystemTheme();

    window
      .matchMedia('(prefers-color-scheme: dark)')
      .addEventListener('change', applySystemTheme);

    return () => {
      window
        .matchMedia('(prefers-color-scheme: dark)')
        .removeEventListener('change', applySystemTheme);
    };
  }, [selectedTheme]);

  return resolvedTheme;
};

export default useTheme;

export const useThemeColor = (color: string) => {
  const theme = useTheme();
  const themeVariables = themes[theme];

  // @ts-expect-error - themeVariables is a string indexable type
  return themeVariables[color];
};
