import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { clearPendingChanges } from 'Store/Actions/baseActions';
import { cancelSaveImportList, cancelTestImportList } from 'Store/Actions/settingsActions';
import EditImportListModal from './EditImportListModal';

function createMapDispatchToProps(dispatch, props) {
  const section = 'settings.importLists';

  return {
    dispatchClearPendingChanges() {
      dispatch(clearPendingChanges({ section }));
    },

    dispatchCancelTestImportList() {
      dispatch(cancelTestImportList({ section }));
    },

    dispatchCancelSaveImportList() {
      dispatch(cancelSaveImportList({ section }));
    }
  };
}

class EditImportListModalConnector extends Component {

  //
  // Listeners

  onModalClose = () => {
    this.props.dispatchClearPendingChanges();
    this.props.dispatchCancelTestImportList();
    this.props.dispatchCancelSaveImportList();
    this.props.onModalClose();
  };

  //
  // Render

  render() {
    const {
      dispatchClearPendingChanges,
      dispatchCancelTestImportList,
      dispatchCancelSaveImportList,
      ...otherProps
    } = this.props;

    return (
      <EditImportListModal
        {...otherProps}
        onModalClose={this.onModalClose}
      />
    );
  }
}

EditImportListModalConnector.propTypes = {
  onModalClose: PropTypes.func.isRequired,
  dispatchClearPendingChanges: PropTypes.func.isRequired,
  dispatchCancelTestImportList: PropTypes.func.isRequired,
  dispatchCancelSaveImportList: PropTypes.func.isRequired
};

export default connect(null, createMapDispatchToProps)(EditImportListModalConnector);
