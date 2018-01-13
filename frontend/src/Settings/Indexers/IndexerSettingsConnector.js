import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { testAllIndexers } from 'Store/Actions/settingsActions';
import IndexerSettings from './IndexerSettings';

function createMapStateToProps() {
  return createSelector(
    (state) => state.settings.indexers.isTestingAll,
    (isTestingAll) => {
      return {
        isTestingAll
      };
    }
  );
}

const mapDispatchToProps = {
  dispatchTestAllIndexers: testAllIndexers
};

export default connect(createMapStateToProps, mapDispatchToProps)(IndexerSettings);
