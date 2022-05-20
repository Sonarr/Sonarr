import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import * as commandNames from 'Commands/commandNames';
import createCommandExecutingSelector from 'Store/Selectors/createCommandExecutingSelector';
import Quality from './Quality';

function createMapStateToProps() {
  return createSelector(
    createCommandExecutingSelector(commandNames.RESET_QUALITY_DEFINITIONS),
    (isResettingQualityDefinitions) => {
      return {
        isResettingQualityDefinitions
      };
    }
  );
}

class QualityConnector extends Component {

  //
  // Render

  render() {
    return (
      <Quality
        {...this.props}
      />
    );
  }
}

QualityConnector.propTypes = {
  isResettingQualityDefinitions: PropTypes.bool.isRequired
};

export default connect(createMapStateToProps)(QualityConnector);
