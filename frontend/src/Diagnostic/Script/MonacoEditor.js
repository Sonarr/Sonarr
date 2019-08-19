import ReactMonacoEditor from 'react-monaco-editor';
import shallowEqual from 'shallowequal';

// All editor features -> 7.56 MiB
// import 'monaco-editor/esm/vs/editor/editor.all';

// Only the needed editor features -> 6.88 MiB
import 'monaco-editor/esm/vs/editor/browser/controller/coreCommands';
import 'monaco-editor/esm/vs/editor/browser/widget/codeEditorWidget';
// import 'monaco-editor/esm/vs/editor/browser/widget/diffEditorWidget';
// import 'monaco-editor/esm/vs/editor/browser/widget/diffNavigator';
import 'monaco-editor/esm/vs/editor/contrib/bracketMatching/bracketMatching';
import 'monaco-editor/esm/vs/editor/contrib/caretOperations/caretOperations';
import 'monaco-editor/esm/vs/editor/contrib/caretOperations/transpose';
// import 'monaco-editor/esm/vs/editor/contrib/clipboard/clipboard';
// import 'monaco-editor/esm/vs/editor/contrib/codeAction/codeActionContributions';
// import 'monaco-editor/esm/vs/editor/contrib/codelens/codelensController';
// import 'monaco-editor/esm/vs/editor/contrib/colorPicker/colorDetector';
import 'monaco-editor/esm/vs/editor/contrib/comment/comment';
import 'monaco-editor/esm/vs/editor/contrib/contextmenu/contextmenu';
import 'monaco-editor/esm/vs/editor/contrib/cursorUndo/cursorUndo';
import 'monaco-editor/esm/vs/editor/contrib/dnd/dnd';
import 'monaco-editor/esm/vs/editor/contrib/find/findController';
import 'monaco-editor/esm/vs/editor/contrib/folding/folding';
import 'monaco-editor/esm/vs/editor/contrib/fontZoom/fontZoom';
// import 'monaco-editor/esm/vs/editor/contrib/format/formatActions';
// import 'monaco-editor/esm/vs/editor/contrib/goToDefinition/goToDefinitionCommands';
// import 'monaco-editor/esm/vs/editor/contrib/goToDefinition/goToDefinitionMouse';
// import 'monaco-editor/esm/vs/editor/contrib/gotoError/gotoError';
import 'monaco-editor/esm/vs/editor/contrib/hover/hover';
import 'monaco-editor/esm/vs/editor/contrib/inPlaceReplace/inPlaceReplace';
import 'monaco-editor/esm/vs/editor/contrib/linesOperations/linesOperations';
// import 'monaco-editor/esm/vs/editor/contrib/links/links';
import 'monaco-editor/esm/vs/editor/contrib/multicursor/multicursor';
// import 'monaco-editor/esm/vs/editor/contrib/parameterHints/parameterHints';
// import 'monaco-editor/esm/vs/editor/contrib/referenceSearch/referenceSearch';
import 'monaco-editor/esm/vs/editor/contrib/rename/rename';
import 'monaco-editor/esm/vs/editor/contrib/smartSelect/smartSelect';
// import 'monaco-editor/esm/vs/editor/contrib/snippet/snippetController2';
// import 'monaco-editor/esm/vs/editor/contrib/suggest/suggestController';
// import 'monaco-editor/esm/vs/editor/contrib/tokenization/tokenization';
// import 'monaco-editor/esm/vs/editor/contrib/toggleTabFocusMode/toggleTabFocusMode';
import 'monaco-editor/esm/vs/editor/contrib/wordHighlighter/wordHighlighter';
import 'monaco-editor/esm/vs/editor/contrib/wordOperations/wordOperations';
import 'monaco-editor/esm/vs/editor/contrib/wordPartOperations/wordPartOperations';

// csharp&json language
import 'monaco-editor/esm/vs/basic-languages/csharp/csharp';
import 'monaco-editor/esm/vs/basic-languages/csharp/csharp.contribution';
import 'monaco-editor/esm/vs/language/json/monaco.contribution';
import 'monaco-editor/esm/vs/language/json/jsonWorker';
import 'monaco-editor/esm/vs/language/json/jsonMode';

// Create a WebWorker from a blob rather than an url
import * as EditorWorker from 'monaco-editor/esm/vs/editor/editor.worker';
import * as JsonWorker from 'monaco-editor/esm/vs/language/json/json.worker';

self.MonacoEnvironment = {
  getWorker: (moduleId, label) => {
    if (label === 'editorWorkerService') {
      return new EditorWorker();
    }
    if (label === 'json') {
      return new JsonWorker();
    }
    return null;
  }
};

class MonacoEditor extends ReactMonacoEditor {

  // ReactMonacoEditor should've been PureComponent
  shouldComponentUpdate(nextProps, nextState) {
    if (!shallowEqual(this.props, nextProps)) {
      return true;
    }

    return false;
  }
}

export default MonacoEditor;
