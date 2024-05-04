import React, { Fragment, ReactNode, useCallback, useEffect } from 'react';
import { useSelector } from 'react-redux';
import { createSelector } from 'reselect';
import themes from 'Styles/Themes';
import AppState from './State/AppState';

interface ApplyThemeProps {
  children: ReactNode;
}

function createThemeSelector() {
  return createSelector(
    (state: AppState) => state.settings.ui.item.theme || window.Sonarr.theme,
    (theme) => {
      return theme;
    }
  );
}

function ApplyTheme({ children }: ApplyThemeProps) {
  const theme = useSelector(createThemeSelector());

  const updateCSSVariables = useCallback(() => {
    Object.entries(themes[theme]).forEach(([key, value]) => {
      document.documentElement.style.setProperty(`--${key}`, value);
    });
  }, [theme]);

  // On Component Mount and Component Update
  useEffect(() => {
    updateCSSVariables();
  }, [updateCSSVariables, theme]);

  return <Fragment>{children}</Fragment>;
}

export default ApplyTheme;
