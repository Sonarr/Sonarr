import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { fetchStatus, updateScript, validateScript, executeScript } from 'Store/Actions/diagnosticActions';
import ScriptConsole from './ScriptConsole';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import Alert from 'Components/Alert';
import { kinds } from 'Helpers/Props';
import PageContent from 'Components/Page/PageContent';

function createMapStateToProps() {
  return createSelector(
    (state) => state.diagnostic,
    (diag) => {
      return {
        isStatusPopulated: diag.status.isPopulated,
        isScriptConsoleEnabled: diag.status.item.scriptConsoleEnabled,
        isExecuting: diag.script.isExecuting || false,
        isDebugging: diag.script.isDebugging || false,
        isValidating: diag.script.isValidating,
        code: diag.script.code,
        result: diag.script.result,
        validation: diag.script.validation,
        error: diag.script.error
      };
    }
  );
}

const mapDispatchToProps = {
  fetchStatus,
  updateScript,
  validateScript,
  executeScript
};

class ScriptConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    if (!this.props.isStatusPopulated) {
      this.props.fetchStatus();
    }
  }

  //
  // Render

  render() {
    if (!this.props.isStatusPopulated) {
      return (
        <PageContent>
          <LoadingIndicator />
        </PageContent>
      );
    } else if (!this.props.isScriptConsoleEnabled) {
      return (
        <PageContent>
          <Alert kind={kinds.WARNING}>
            Diagnostic Scripting is disabled
          </Alert>
        </PageContent>
      );
    }

    return (
      <ScriptConsole
        {...this.props}
      />
    );
  }
}

ScriptConnector.propTypes = {
  isStatusPopulated: PropTypes.bool.isRequired,
  isScriptConsoleEnabled: PropTypes.bool,
  isExecuting: PropTypes.bool.isRequired,
  isDebugging: PropTypes.bool.isRequired,
  isValidating: PropTypes.bool.isRequired,
  code: PropTypes.string,
  result: PropTypes.object,
  error: PropTypes.object,
  validation: PropTypes.object,
  fetchStatus: PropTypes.func.isRequired,
  updateScript: PropTypes.func.isRequired,
  validateScript: PropTypes.func.isRequired,
  executeScript: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(ScriptConnector);
