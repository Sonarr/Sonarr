import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import selectSettings from 'Store/Selectors/selectSettings';
import { setImportListExclusionValue, saveImportListExclusion } from 'Store/Actions/settingsActions';
import EditImportListExclusionModalContent from './EditImportListExclusionModalContent';

const newImportListExclusion = {
  title: '',
  tvdbId: 0
};

function createImportListExclusionSelector() {
  return createSelector(
    (state, { id }) => id,
    (state) => state.settings.importListExclusions,
    (id, importListExclusions) => {
      const {
        isFetching,
        error,
        isSaving,
        saveError,
        pendingChanges,
        items
      } = importListExclusions;

      const mapping = id ? _.find(items, { id }) : newImportListExclusion;
      const settings = selectSettings(mapping, pendingChanges, saveError);

      return {
        id,
        isFetching,
        error,
        isSaving,
        saveError,
        item: settings.settings,
        ...settings
      };
    }
  );
}

function createMapStateToProps() {
  return createSelector(
    createImportListExclusionSelector(),
    (importListExclusion) => {
      return {
        ...importListExclusion
      };
    }
  );
}

const mapDispatchToProps = {
  setImportListExclusionValue,
  saveImportListExclusion
};

class EditImportListExclusionModalContentConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    if (!this.props.id) {
      Object.keys(newImportListExclusion).forEach((name) => {
        this.props.setImportListExclusionValue({
          name,
          value: newImportListExclusion[name]
        });
      });
    }
  }

  componentDidUpdate(prevProps, prevState) {
    if (prevProps.isSaving && !this.props.isSaving && !this.props.saveError) {
      this.props.onModalClose();
    }
  }

  //
  // Listeners

  onInputChange = ({ name, value }) => {
    this.props.setImportListExclusionValue({ name, value });
  }

  onSavePress = () => {
    this.props.saveImportListExclusion({ id: this.props.id });
  }

  //
  // Render

  render() {
    return (
      <EditImportListExclusionModalContent
        {...this.props}
        onSavePress={this.onSavePress}
        onInputChange={this.onInputChange}
      />
    );
  }
}

EditImportListExclusionModalContentConnector.propTypes = {
  id: PropTypes.number,
  isSaving: PropTypes.bool.isRequired,
  saveError: PropTypes.object,
  item: PropTypes.object.isRequired,
  setImportListExclusionValue: PropTypes.func.isRequired,
  saveImportListExclusion: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(EditImportListExclusionModalContentConnector);
