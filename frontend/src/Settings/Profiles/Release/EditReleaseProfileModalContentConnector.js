import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { saveReleaseProfile, setReleaseProfileValue } from 'Store/Actions/settingsActions';
import selectSettings from 'Store/Selectors/selectSettings';
import EditReleaseProfileModalContent from './EditReleaseProfileModalContent';

const newReleaseProfile = {
  enabled: true,
  required: [],
  ignored: [],
  includePreferredWhenRenaming: false,
  tags: [],
  indexerId: 0
};

function createMapStateToProps() {
  return createSelector(
    (state, { id }) => id,
    (state) => state.settings.releaseProfiles,
    (id, releaseProfiles) => {
      const {
        isFetching,
        error,
        isSaving,
        saveError,
        pendingChanges,
        items
      } = releaseProfiles;

      const profile = id ? _.find(items, { id }) : newReleaseProfile;
      const settings = selectSettings(profile, pendingChanges, saveError);

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

const mapDispatchToProps = {
  setReleaseProfileValue,
  saveReleaseProfile
};

class EditReleaseProfileModalContentConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    if (!this.props.id) {
      Object.keys(newReleaseProfile).forEach((name) => {
        this.props.setReleaseProfileValue({
          name,
          value: newReleaseProfile[name]
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
    this.props.setReleaseProfileValue({ name, value });
  };

  onSavePress = () => {
    this.props.saveReleaseProfile({ id: this.props.id });
  };

  //
  // Render

  render() {
    return (
      <EditReleaseProfileModalContent
        {...this.props}
        onSavePress={this.onSavePress}
        onTestPress={this.onTestPress}
        onInputChange={this.onInputChange}
        onFieldChange={this.onFieldChange}
      />
    );
  }
}

EditReleaseProfileModalContentConnector.propTypes = {
  id: PropTypes.number,
  isFetching: PropTypes.bool.isRequired,
  isSaving: PropTypes.bool.isRequired,
  saveError: PropTypes.object,
  item: PropTypes.object.isRequired,
  setReleaseProfileValue: PropTypes.func.isRequired,
  saveReleaseProfile: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(EditReleaseProfileModalContentConnector);
