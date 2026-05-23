import { useLayoutEffect } from 'react';
import useTheme from 'Helpers/Hooks/useTheme';
import { useUiSettingsValues } from 'Settings/UI/useUiSettings';

function ApplyTheme() {
  const theme = useTheme();
  const { enableColorImpairedMode } = useUiSettingsValues();

  useLayoutEffect(() => {
    document.documentElement.dataset.theme = theme;
  }, [theme]);

  useLayoutEffect(() => {
    if (enableColorImpairedMode) {
      document.documentElement.dataset.colorImpaired = '';
    } else {
      delete document.documentElement.dataset.colorImpaired;
    }
  }, [enableColorImpairedMode]);

  return null;
}

export default ApplyTheme;
