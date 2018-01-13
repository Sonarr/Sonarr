import { connect } from 'react-redux';
import ConnectionLostModal from './ConnectionLostModal';

function createMapDispatchToProps(dispatch, props) {
  return {
    onModalClose() {
      location.reload();
    }
  };
}

export default connect(undefined, createMapDispatchToProps)(ConnectionLostModal);
