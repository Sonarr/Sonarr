import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { testAllImportLists } from 'Store/Actions/settingsActions';
import ImportListSettings from './ImportListSettings';

function createMapStateToProps() {
  return createSelector(
    (state) => state.settings.importLists.isTestingAll,
    (isTestingAll) => {
      return {
        isTestingAll
      };
    }
  );
}

const mapDispatchToProps = {
  dispatchTestAllImportLists: testAllImportLists
};

export default connect(createMapStateToProps, mapDispatchToProps)(ImportListSettings);
