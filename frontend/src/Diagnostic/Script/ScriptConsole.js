import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component, lazy, Suspense } from 'react';
import { icons } from 'Helpers/Props';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import PageToolbar from 'Components/Page/Toolbar/PageToolbar';
import PageContent from 'Components/Page/PageContent';
import PageToolbarSection from 'Components/Page/Toolbar/PageToolbarSection';
import PageToolbarButton from 'Components/Page/Toolbar/PageToolbarButton';
import styles from './ScriptConsole.css';

// Lazy load the Monaco Editor since it's a big bundle
const MonacoEditor = lazy(() => import(/* webpackChunkName: "monaco-editor" */ './MonacoEditor'));

const DefaultOptions = {
  selectOnLineNumbers: true,
  scrollBeyondLastLine: false
};
const DefaultResultOptions = {
  ...DefaultOptions,
  readOnly: true
};

class ScriptConsole extends Component {

  //
  // Lifecycle

  editorDidMount = (editor, monaco) => {
    console.log('editorDidMount', editor);
    editor.focus();
    this.monaco = monaco;
    this.editor = editor;

    this.updateValidation(this.props.validation);
  }

  updateValidation(validation) {
    if (!this.monaco) {
      return;
    }

    let diagnostics = [];

    if (validation && validation.errorDiagnostics) {
      diagnostics = validation.errorDiagnostics;
    }

    const model = this.editor.getModel();

    this.monaco.editor.setModelMarkers(model, 'editor', diagnostics);
  }

  onChange = (newValue, e) => {
    this.props.updateScript({ code: newValue });

    this.validateCode();
  }

  validateCode = _.debounce(() => {
    const code = this.props.code;
    this.props.validateScript({ code });
  }, 250, { leading: false, trailing: true })

  onExecuteScriptPress = () => {
    const code = this.props.code;
    this.props.executeScript({ code });
  }

  onDebugScriptPress = () => {
    const code = this.props.code;
    this.props.executeScript({ code, debug: true });
  }

  //
  // Render
  render() {
    const code = this.props.code;
    const result = JSON.stringify(this.props.result, null, 2);

    this.updateValidation(this.props.validation);

    return (
      <PageContent>
        <PageToolbar>
          <PageToolbarSection>
            <PageToolbarButton
              label="Run"
              iconName={this.props.isExecuting ? icons.REFRESH : icons.SCRIPT_RUN}
              isSpinning={this.props.isExecuting}
              onPress={this.onExecuteScriptPress}
            />
            <PageToolbarButton
              label="Debug"
              iconName={this.props.isDebugging ? icons.REFRESH : icons.SCRIPT_DEBUG}
              isSpinning={this.props.isDebugging}
              onPress={this.onDebugScriptPress}
            />
          </PageToolbarSection>
        </PageToolbar>
        <Suspense fallback={<LoadingIndicator />}>
          <div className={styles.split}>
            <MonacoEditor
              language="csharp"
              theme="vs-light"
              width="50%"
              value={code}
              options={DefaultOptions}
              onChange={this.onChange}
              editorDidMount={this.editorDidMount}
            />
            <MonacoEditor
              language="json"
              theme="vs-light"
              width="50%"
              value={result}
              options={DefaultResultOptions}
            />
          </div>
        </Suspense>
      </PageContent>
    );
  }
}

ScriptConsole.propTypes = {
  isExecuting: PropTypes.bool.isRequired,
  isDebugging: PropTypes.bool.isRequired,
  isValidating: PropTypes.bool.isRequired,
  code: PropTypes.string,
  result: PropTypes.object,
  error: PropTypes.object,
  validation: PropTypes.object,
  updateScript: PropTypes.func.isRequired,
  validateScript: PropTypes.func.isRequired,
  executeScript: PropTypes.func.isRequired
};

export default ScriptConsole;
