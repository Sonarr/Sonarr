import { useEffect, useLayoutEffect, useState } from 'react';
import { useUiSettingsValues } from 'Settings/UI/useUiSettings';

const useTheme = (): 'dark' | 'light' => {
  const { theme } = useUiSettingsValues();
  const selectedTheme = theme ?? window.Sonarr.theme;
  const [resolvedTheme, setResolvedTheme] = useState(() => {
    if (selectedTheme === 'auto') {
      return window.matchMedia('(prefers-color-scheme: dark)').matches
        ? 'dark'
        : 'light';
    }

    return selectedTheme;
  });

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

const readThemeVar = (color: string) =>
  getComputedStyle(document.documentElement)
    .getPropertyValue(`--${color}`)
    .trim();

export const useThemeColor = (color: string) => {
  const theme = useTheme();
  const [value, setValue] = useState(() => readThemeVar(color));

  useLayoutEffect(() => {
    setValue(readThemeVar(color));
  }, [color, theme]);

  return value;
};
