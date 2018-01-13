import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { restoreBackup, restart } from 'Store/Actions/systemActions';
import RestoreBackupModalContent from './RestoreBackupModalContent';

function createMapStateToProps() {
  return createSelector(
    (state) => state.system.backups,
    (state) => state.app.isRestarting,
    (backups, isRestarting) => {
      const {
        isRestoring,
        restoreError
      } = backups;

      return {
        isRestoring,
        restoreError,
        isRestarting
      };
    }
  );
}

function createMapDispatchToProps(dispatch, props) {
  return {
    onRestorePress(payload) {
      dispatch(restoreBackup(payload));
    },

    dispatchRestart() {
      dispatch(restart());
    }
  };
}

export default connect(createMapStateToProps, createMapDispatchToProps)(RestoreBackupModalContent);
