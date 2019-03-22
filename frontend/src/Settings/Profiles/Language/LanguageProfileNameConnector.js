import PropTypes from 'prop-types';
import React from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createLanguageProfileSelector from 'Store/Selectors/createLanguageProfileSelector';

function createMapStateToProps() {
  return createSelector(
    createLanguageProfileSelector(),
    (languageProfile) => {
      return {
        name: languageProfile.name
      };
    }
  );
}

function LanguageProfileNameConnector({ name, ...otherProps }) {
  return (
    <span>
      {name}
    </span>
  );
}

LanguageProfileNameConnector.propTypes = {
  languageProfileId: PropTypes.number.isRequired,
  name: PropTypes.string.isRequired
};

export default connect(createMapStateToProps)(LanguageProfileNameConnector);
