import PropTypes from 'prop-types';
import React from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createQualityProfileSelector from 'Store/Selectors/createQualityProfileSelector';
import translate from 'Utilities/String/translate';
import styles from './QualityProfileNameConnector.css';

function createMapStateToProps() {
  return createSelector(
    createQualityProfileSelector(),
    (qualityProfile) => {
      return {
        name: qualityProfile?.name
      };
    }
  );
}

function QualityProfileNameConnector({ name }) {
  if (name) {
    return (
      <span>{name}</span>
    );
  }

  return (
    <span className={styles.qualityProfileUnknown}>
      {translate('None')}
    </span>
  );
}

QualityProfileNameConnector.propTypes = {
  qualityProfileId: PropTypes.number.isRequired,
  name: PropTypes.string.isRequired
};

export default connect(createMapStateToProps)(QualityProfileNameConnector);
