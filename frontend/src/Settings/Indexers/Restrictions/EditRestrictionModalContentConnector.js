import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import selectSettings from 'Store/Selectors/selectSettings';
import { setRestrictionValue, saveRestriction } from 'Store/Actions/settingsActions';
import EditRestrictionModalContent from './EditRestrictionModalContent';

const newRestriction = {
  required: '',
  ignored: '',
  tags: []
};

function createMapStateToProps() {
  return createSelector(
    (state, { id }) => id,
    (state) => state.settings.restrictions,
    (id, restrictions) => {
      const {
        isFetching,
        error,
        isSaving,
        saveError,
        pendingChanges,
        items
      } = restrictions;

      const profile = id ? _.find(items, { id }) : newRestriction;
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
  setRestrictionValue,
  saveRestriction
};

class EditRestrictionModalContentConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    if (!this.props.id) {
      Object.keys(newRestriction).forEach((name) => {
        this.props.setRestrictionValue({
          name,
          value: newRestriction[name]
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
    this.props.setRestrictionValue({ name, value });
  }

  onSavePress = () => {
    this.props.saveRestriction({ id: this.props.id });
  }

  //
  // Render

  render() {
    return (
      <EditRestrictionModalContent
        {...this.props}
        onSavePress={this.onSavePress}
        onTestPress={this.onTestPress}
        onInputChange={this.onInputChange}
        onFieldChange={this.onFieldChange}
      />
    );
  }
}

EditRestrictionModalContentConnector.propTypes = {
  id: PropTypes.number,
  isFetching: PropTypes.bool.isRequired,
  isSaving: PropTypes.bool.isRequired,
  saveError: PropTypes.object,
  item: PropTypes.object.isRequired,
  setRestrictionValue: PropTypes.func.isRequired,
  saveRestriction: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(EditRestrictionModalContentConnector);
