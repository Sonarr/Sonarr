import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createSystemStatusSelector from 'Store/Selectors/createSystemStatusSelector';
import KeyboardShortcutsModalContent from './KeyboardShortcutsModalContent';

function createMapStateToProps() {
  return createSelector(
    createSystemStatusSelector(),
    (systemStatus) => {
      return {
        isOsx: systemStatus.isOsx
      };
    }
  );
}

export default connect(createMapStateToProps)(KeyboardShortcutsModalContent);
