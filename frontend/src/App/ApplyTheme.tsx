import { useLayoutEffect } from 'react';
import useTheme from 'Helpers/Hooks/useTheme';
import { useUiSettingsValues } from 'Settings/UI/useUiSettings';

function ApplyTheme() {
  const theme = useTheme();
  const { enableColorImpairedMode, headerColor } = useUiSettingsValues();

  useLayoutEffect(() => {
    document.documentElement.dataset.theme = theme;
    document.documentElement.style.removeProperty('--pageBackground');
  }, [theme]);

  useLayoutEffect(() => {
    if (enableColorImpairedMode) {
      document.documentElement.dataset.colorImpaired = '';
    } else {
      delete document.documentElement.dataset.colorImpaired;
    }
  }, [enableColorImpairedMode]);

  useLayoutEffect(() => {
    if (headerColor === 'gradient') {
      document.documentElement.dataset.headerColor = headerColor;
    } else {
      delete document.documentElement.dataset.headerColor;
    }
  }, [headerColor]);

  return null;
}

export default ApplyTheme;
