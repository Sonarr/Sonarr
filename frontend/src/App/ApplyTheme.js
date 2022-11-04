import PropTypes from 'prop-types';
import React, { Fragment, useCallback, useEffect } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import themes from 'Styles/Themes';

function createMapStateToProps() {
  return createSelector(
    (state) => state.settings.ui.item.theme || window.Sonarr.theme,
    (
      theme
    ) => {
      return {
        theme
      };
    }
  );
}

function ApplyTheme({ theme, children }) {
  // Update the CSS Variables

  const updateCSSVariables = useCallback(() => {
    const arrayOfVariableKeys = Object.keys(themes[theme]);
    const arrayOfVariableValues = Object.values(themes[theme]);

    // Loop through each array key and set the CSS Variables
    arrayOfVariableKeys.forEach((cssVariableKey, index) => {
      // Based on our snippet from MDN
      document.documentElement.style.setProperty(
        `--${cssVariableKey}`,
        arrayOfVariableValues[index]
      );
    });
  }, [theme]);

  // On Component Mount and Component Update
  useEffect(() => {
    updateCSSVariables(theme);
  }, [updateCSSVariables, theme]);

  return <Fragment>{children}</Fragment>;
}

ApplyTheme.propTypes = {
  theme: PropTypes.string.isRequired,
  children: PropTypes.object.isRequired
};

export default connect(createMapStateToProps)(ApplyTheme);
