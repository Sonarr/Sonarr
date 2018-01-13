import PropTypes from 'prop-types';
import React from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createQualityProfileSelector from 'Store/Selectors/createQualityProfileSelector';

function createMapStateToProps() {
  return createSelector(
    createQualityProfileSelector(),
    (qualityProfile) => {
      return {
        name: qualityProfile.name
      };
    }
  );
}

function QualityProfileNameConnector({ name, ...otherProps }) {
  return (
    <span>
      {name}
    </span>
  );
}

QualityProfileNameConnector.propTypes = {
  qualityProfileId: PropTypes.number.isRequired,
  name: PropTypes.string.isRequired
};

export default connect(createMapStateToProps)(QualityProfileNameConnector);
