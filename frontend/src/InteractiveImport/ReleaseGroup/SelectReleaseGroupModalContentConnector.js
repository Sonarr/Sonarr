import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { reprocessInteractiveImportItems, updateInteractiveImportItems } from 'Store/Actions/interactiveImportActions';
import SelectReleaseGroupModalContent from './SelectReleaseGroupModalContent';

const mapDispatchToProps = {
  dispatchUpdateInteractiveImportItems: updateInteractiveImportItems,
  dispatchReprocessInteractiveImportItems: reprocessInteractiveImportItems
};

class SelectReleaseGroupModalContentConnector extends Component {

  //
  // Listeners

  onReleaseGroupSelect = ({ releaseGroup }) => {
    const {
      ids,
      dispatchUpdateInteractiveImportItems,
      dispatchReprocessInteractiveImportItems
    } = this.props;

    dispatchUpdateInteractiveImportItems({
      ids,
      releaseGroup
    });

    dispatchReprocessInteractiveImportItems({ ids });

    this.props.onModalClose(true);
  };

  //
  // Render

  render() {
    return (
      <SelectReleaseGroupModalContent
        {...this.props}
        onReleaseGroupSelect={this.onReleaseGroupSelect}
      />
    );
  }
}

SelectReleaseGroupModalContentConnector.propTypes = {
  ids: PropTypes.arrayOf(PropTypes.number).isRequired,
  dispatchUpdateInteractiveImportItems: PropTypes.func.isRequired,
  dispatchReprocessInteractiveImportItems: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default connect(null, mapDispatchToProps)(SelectReleaseGroupModalContentConnector);
