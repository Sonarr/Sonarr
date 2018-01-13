import { connect } from 'react-redux';
import { clearRestoreBackup } from 'Store/Actions/systemActions';
import RestoreBackupModal from './RestoreBackupModal';

function createMapDispatchToProps(dispatch, props) {
  return {
    onModalClose() {
      dispatch(clearRestoreBackup());

      props.onModalClose();
    }
  };
}

export default connect(null, createMapDispatchToProps)(RestoreBackupModal);
