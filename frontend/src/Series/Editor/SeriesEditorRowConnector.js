import PropTypes from 'prop-types';
import React from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createLanguageProfileSelector from 'Store/Selectors/createLanguageProfileSelector';
import createQualityProfileSelector from 'Store/Selectors/createQualityProfileSelector';
import SeriesEditorRow from './SeriesEditorRow';

function createMapStateToProps() {
  return createSelector(
    createLanguageProfileSelector(),
    createQualityProfileSelector(),
    (languageProfile, qualityProfile) => {
      return {
        languageProfile,
        qualityProfile
      };
    }
  );
}

function SeriesEditorRowConnector(props) {
  return (
    <SeriesEditorRow
      {...props}
    />
  );
}

SeriesEditorRowConnector.propTypes = {
  qualityProfileId: PropTypes.number.isRequired
};

export default connect(createMapStateToProps)(SeriesEditorRowConnector);
