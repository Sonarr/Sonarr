import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { clearPendingChanges } from 'Store/Actions/baseActions';
import { cancelTestIndexer, cancelSaveIndexer } from 'Store/Actions/settingsActions';
import EditIndexerModal from './EditIndexerModal';

function createMapDispatchToProps(dispatch, props) {
  const section = 'settings.indexers';

  return {
    dispatchClearPendingChanges() {
      dispatch(clearPendingChanges({ section }));
    },

    dispatchCancelTestIndexer() {
      dispatch(cancelTestIndexer({ section }));
    },

    dispatchCancelSaveIndexer() {
      dispatch(cancelSaveIndexer({ section }));
    }
  };
}

class EditIndexerModalConnector extends Component {

  //
  // Listeners

  onModalClose = () => {
    this.props.dispatchClearPendingChanges();
    this.props.dispatchCancelTestIndexer();
    this.props.dispatchCancelSaveIndexer();
    this.props.onModalClose();
  }

  //
  // Render

  render() {
    const {
      dispatchClearPendingChanges,
      dispatchCancelTestIndexer,
      dispatchCancelSaveIndexer,
      ...otherProps
    } = this.props;

    return (
      <EditIndexerModal
        {...otherProps}
        onModalClose={this.onModalClose}
      />
    );
  }
}

EditIndexerModalConnector.propTypes = {
  onModalClose: PropTypes.func.isRequired,
  dispatchClearPendingChanges: PropTypes.func.isRequired,
  dispatchCancelTestIndexer: PropTypes.func.isRequired,
  dispatchCancelSaveIndexer: PropTypes.func.isRequired
};

export default connect(null, createMapDispatchToProps)(EditIndexerModalConnector);
