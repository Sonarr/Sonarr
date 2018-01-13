import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import selectSettings from 'Store/Selectors/selectSettings';
import { setMetadataValue, setMetadataFieldValue, saveMetadata } from 'Store/Actions/settingsActions';
import EditMetadataModalContent from './EditMetadataModalContent';

function createMapStateToProps() {
  return createSelector(
    (state, { id }) => id,
    (state) => state.settings.metadata,
    (id, metadata) => {
      const {
        isSaving,
        saveError,
        pendingChanges,
        items
      } = metadata;

      const settings = selectSettings(_.find(items, { id }), pendingChanges, saveError);

      return {
        id,
        isSaving,
        saveError,
        item: settings.settings,
        ...settings
      };
    }
  );
}

const mapDispatchToProps = {
  setMetadataValue,
  setMetadataFieldValue,
  saveMetadata
};

class EditMetadataModalContentConnector extends Component {

  //
  // Lifecycle

  componentDidUpdate(prevProps, prevState) {
    if (prevProps.isSaving && !this.props.isSaving && !this.props.saveError) {
      this.props.onModalClose();
    }
  }

  //
  // Listeners

  onInputChange = ({ name, value }) => {
    this.props.setMetadataValue({ name, value });
  }

  onFieldChange = ({ name, value }) => {
    this.props.setMetadataFieldValue({ name, value });
  }

  onSavePress = () => {
    this.props.saveMetadata({ id: this.props.id });
  }

  //
  // Render

  render() {
    return (
      <EditMetadataModalContent
        {...this.props}
        onSavePress={this.onSavePress}
        onInputChange={this.onInputChange}
        onFieldChange={this.onFieldChange}
      />
    );
  }
}

EditMetadataModalContentConnector.propTypes = {
  id: PropTypes.number,
  isSaving: PropTypes.bool.isRequired,
  saveError: PropTypes.object,
  item: PropTypes.object.isRequired,
  setMetadataValue: PropTypes.func.isRequired,
  setMetadataFieldValue: PropTypes.func.isRequired,
  saveMetadata: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(EditMetadataModalContentConnector);
