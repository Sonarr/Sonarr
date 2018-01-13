import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { testAllDownloadClients } from 'Store/Actions/settingsActions';
import DownloadClientSettings from './DownloadClientSettings';

function createMapStateToProps() {
  return createSelector(
    (state) => state.settings.downloadClients.isTestingAll,
    (isTestingAll) => {
      return {
        isTestingAll
      };
    }
  );
}

const mapDispatchToProps = {
  dispatchTestAllDownloadClients: testAllDownloadClients
};

export default connect(createMapStateToProps, mapDispatchToProps)(DownloadClientSettings);
