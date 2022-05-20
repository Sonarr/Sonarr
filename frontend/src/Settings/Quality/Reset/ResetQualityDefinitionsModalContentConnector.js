import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import * as commandNames from 'Commands/commandNames';
import { executeCommand } from 'Store/Actions/commandActions';
import createCommandExecutingSelector from 'Store/Selectors/createCommandExecutingSelector';
import ResetQualityDefinitionsModalContent from './ResetQualityDefinitionsModalContent';

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

const mapDispatchToProps = {
  executeCommand
};

class ResetQualityDefinitionsModalContentConnector extends Component {

  //
  // Listeners

  onResetQualityDefinitions = (resetTitles) => {
    this.props.executeCommand({ name: commandNames.RESET_QUALITY_DEFINITIONS, resetTitles });
    this.props.onModalClose(true);
  };

  //
  // Render

  render() {
    return (
      <ResetQualityDefinitionsModalContent
        {...this.props}
        onResetQualityDefinitions={this.onResetQualityDefinitions}
      />
    );
  }
}

ResetQualityDefinitionsModalContentConnector.propTypes = {
  onModalClose: PropTypes.func.isRequired,
  isResettingQualityDefinitions: PropTypes.bool.isRequired,
  executeCommand: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(ResetQualityDefinitionsModalContentConnector);
